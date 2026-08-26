using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.Employees;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class EmployeesApiClient(
    HttpClient httpClient,
    IOfficeApiRequestAuthenticator requestAuthenticator) : IEmployeesApiClient
{
    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (isActive.HasValue)
        {
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        string path = query.Count == 0
            ? "api/employees"
            : $"api/employees?{string.Join('&', query)}";
        using HttpResponseMessage response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<EmployeeResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Get,
            $"api/employees/{id}",
            null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public async Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Post,
            "api/employees",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public async Task<EmployeeResponse> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Put,
            $"api/employees/{id}",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/employees/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/employees/{id}/deactivate", cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Delete,
            $"api/employees/{id}",
            null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task PostActionAsync(string path, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await SendAsync(HttpMethod.Post, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (content is not null)
        {
            request.Content = JsonContent.Create(content);
        }

        await requestAuthenticator.ApplyAsync(request, cancellationToken);
        request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");
        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<EmployeeResponse> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<EmployeeResponse>(cancellationToken: cancellationToken)
        ?? throw new ApiClientException(response.StatusCode, "Die API hat keine gültige Antwort geliefert.");

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        var (message, field) = ParseProblem(content);
        throw new ApiClientException(response.StatusCode, message, field);
    }

    private static (string Message, string? Field) ParseProblem(string content)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement root = document.RootElement;
            string? field = root.TryGetProperty("field", out JsonElement fieldElement)
                ? fieldElement.GetString()
                : null;
            if (root.TryGetProperty("errors", out JsonElement errorsElement))
            {
                foreach (JsonProperty error in errorsElement.EnumerateObject())
                {
                    JsonElement first = error.Value.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.String)
                    {
                        return (first.GetString()!, error.Name);
                    }
                }
            }

            if (root.TryGetProperty("detail", out JsonElement detail)
                && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                return (detail.GetString()!, field);
            }
        }
        catch (JsonException)
        {
        }

        return ("Die Anfrage konnte nicht verarbeitet werden.", null);
    }
}
