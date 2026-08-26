using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.CleaningObjects;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class CleaningObjectsApiClient(HttpClient httpClient, IOfficeApiRequestAuthenticator requestAuthenticator)
    : ICleaningObjectsApiClient
{
    public async Task<IReadOnlyList<CleaningObjectResponse>> GetAllAsync(
        string? search, bool? isActive, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (isActive.HasValue) query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        if (customerId.HasValue) query.Add($"customerId={customerId.Value}");
        var path = query.Count == 0 ? "api/objects" : $"api/objects?{string.Join('&', query)}";
        using var response = await SendAsync(HttpMethod.Get, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CleaningObjectResponse>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<CleaningObjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"api/objects/{id}", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public async Task<CleaningObjectResponse> CreateAsync(CreateCleaningObjectRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/objects", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public async Task<CleaningObjectResponse> UpdateAsync(Guid id, UpdateCleaningObjectRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Put, $"api/objects/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/objects/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/objects/{id}/deactivate", cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"api/objects/{id}", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task PostActionAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, path, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? content, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (content is not null) request.Content = JsonContent.Create(content);
        await requestAuthenticator.ApplyAsync(request, cancellationToken);
        request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");
        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<CleaningObjectResponse> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<CleaningObjectResponse>(cancellationToken: cancellationToken)
        ?? throw new ApiClientException(response.StatusCode, "Die API hat keine gültige Antwort geliefert.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = "Die Anfrage konnte nicht verarbeitet werden.";
        string? field = null;
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.TryGetProperty("field", out var fieldElement)) field = fieldElement.GetString();
            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString())) message = detail.GetString()!;
            else if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString())) message = title.GetString()!;
        }
        catch (JsonException) { }
        throw new ApiClientException(response.StatusCode, message, field);
    }
}
