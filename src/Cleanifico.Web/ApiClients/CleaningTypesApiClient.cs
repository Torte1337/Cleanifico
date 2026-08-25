using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.CleaningTypes;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class CleaningTypesApiClient(
    HttpClient httpClient,
    IOfficeApiRequestAuthenticator requestAuthenticator) : ICleaningTypesApiClient
{
    public async Task<IReadOnlyList<CleaningTypeResponse>> GetAllAsync(
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
            ? "api/cleaning-types"
            : $"api/cleaning-types?{string.Join('&', query)}";

        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<CleaningTypeResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<CleaningTypeResponse> CreateAsync(
        CreateCleaningTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "api/cleaning-types",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public async Task<CleaningTypeResponse> UpdateAsync(
        Guid id,
        UpdateCleaningTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            $"api/cleaning-types/{id}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadResponseAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/cleaning-types/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/cleaning-types/{id}/deactivate", cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Delete,
            $"api/cleaning-types/{id}",
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

    private static async Task<CleaningTypeResponse> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<CleaningTypeResponse>(
            cancellationToken: cancellationToken)
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
                    var firstMessage = firstError.ValueKind == JsonValueKind.String
                        ? firstError.GetString()
                        : null;
                    if (!string.IsNullOrWhiteSpace(firstMessage))
                    {
                        return (firstMessage, error.Name);
                    }
                }
            }

            if (root.TryGetProperty("detail", out var detailElement) &&
                !string.IsNullOrWhiteSpace(detailElement.GetString()))
            {
                return (detailElement.GetString()!, field);
            }

            if (root.TryGetProperty("title", out var titleElement) &&
                !string.IsNullOrWhiteSpace(titleElement.GetString()))
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
