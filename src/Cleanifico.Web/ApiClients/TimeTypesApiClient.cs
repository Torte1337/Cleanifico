using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.TimeTypes;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class TimeTypesApiClient(
    HttpClient httpClient,
    IOfficeApiRequestAuthenticator requestAuthenticator) : ITimeTypesApiClient
{
    public async Task<IReadOnlyList<TimeTypeResponse>> GetAllAsync(
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

        var path = query.Count == 0
            ? "api/time-types"
            : $"api/time-types?{string.Join('&', query)}";

        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<TimeTypeResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<TimeTypeResponse> CreateAsync(
        CreateTimeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/time-types", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public async Task<TimeTypeResponse> UpdateAsync(
        Guid id,
        UpdateTimeTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            $"api/time-types/{id}",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/time-types/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/time-types/{id}/deactivate", cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Delete,
            $"api/time-types/{id}",
            null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task PostActionAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, path, null, cancellationToken);
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

    private static async Task<TimeTypeResponse> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<TimeTypeResponse>(cancellationToken: cancellationToken)
        ?? throw new ApiClientException(
            response.StatusCode,
            "Die API hat keine gültige Antwort geliefert.");

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var (message, field) = ParseProblem(content);
        throw new ApiClientException(response.StatusCode, message, field);
    }

    private static (string Message, string? Field) ParseProblem(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ("Die Anfrage konnte nicht verarbeitet werden.", null);
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var field = root.TryGetProperty("field", out var fieldElement)
                ? fieldElement.GetString()
                : null;

            if (root.TryGetProperty("errors", out var errorsElement))
            {
                foreach (var error in errorsElement.EnumerateObject())
                {
                    if (error.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    var firstError = error.Value.EnumerateArray().FirstOrDefault();
                    if (firstError.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(firstError.GetString()))
                    {
                        return (firstError.GetString()!, error.Name);
                    }
                }
            }

            if (root.TryGetProperty("detail", out var detailElement)
                && !string.IsNullOrWhiteSpace(detailElement.GetString()))
            {
                return (detailElement.GetString()!, field);
            }

            if (root.TryGetProperty("title", out var titleElement)
                && !string.IsNullOrWhiteSpace(titleElement.GetString()))
            {
                return (titleElement.GetString()!, field);
            }
        }
        catch (JsonException)
        {
            // Raw non-ProblemDetails content is intentionally not exposed to users.
        }

        return ("Die Anfrage konnte nicht verarbeitet werden.", null);
    }
}
