using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.EmployeeContracts;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class EmployeeContractsApiClient(
    HttpClient httpClient,
    IOfficeApiRequestAuthenticator requestAuthenticator) : IEmployeeContractsApiClient
{
    public async Task<IReadOnlyList<EmployeeContractResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
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

        if (employeeId.HasValue)
        {
            query.Add($"employeeId={employeeId.Value}");
        }

        string path = query.Count == 0
            ? "api/employee-contracts"
            : $"api/employee-contracts?{string.Join('&', query)}";
        using HttpResponseMessage response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<EmployeeContractResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<EmployeeContractResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Get,
            $"api/employee-contracts/{id}",
            null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public async Task<EmployeeContractResponse> CreateAsync(
        CreateEmployeeContractRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Post,
            "api/employee-contracts",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public async Task<EmployeeContractResponse> UpdateAsync(
        Guid id,
        UpdateEmployeeContractRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Put,
            $"api/employee-contracts/{id}",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/employee-contracts/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/employee-contracts/{id}/deactivate", cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Delete,
            $"api/employee-contracts/{id}",
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

    private static async Task<EmployeeContractResponse> ReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<EmployeeContractResponse>(
            cancellationToken: cancellationToken)
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
        string message = "Die Anfrage konnte nicht verarbeitet werden.";
        string? field = null;
        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("field", out JsonElement fieldElement))
            {
                field = fieldElement.GetString();
            }

            if (root.TryGetProperty("errors", out JsonElement errors))
            {
                foreach (JsonProperty error in errors.EnumerateObject())
                {
                    JsonElement first = error.Value.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.String)
                    {
                        message = first.GetString()!;
                        field ??= error.Name;
                        break;
                    }
                }
            }
            else if (root.TryGetProperty("detail", out JsonElement detail)
                && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                message = detail.GetString()!;
            }
        }
        catch (JsonException)
        {
        }

        throw new ApiClientException(response.StatusCode, message, field);
    }
}
