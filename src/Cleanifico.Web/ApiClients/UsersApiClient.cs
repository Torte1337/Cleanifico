using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.Users;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.ApiClients;

public sealed class UsersApiClient(
    HttpClient httpClient,
    IOfficeApiRequestAuthenticator requestAuthenticator) : IUsersApiClient
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "api/users", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<UserResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<RoleResponse>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "api/roles", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<RoleResponse>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/users", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadUserAsync(response, cancellationToken);
    }

    public async Task<UserResponse> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Put, $"api/users/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadUserAsync(response, cancellationToken);
    }

    public async Task<UserResponse> UpdateRolesAsync(
        Guid id,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            $"api/users/{id}/roles",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadUserAsync(response, cancellationToken);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/users/{id}/activate", cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        PostActionAsync($"api/users/{id}/deactivate", cancellationToken);

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

    private static async Task<UserResponse> ReadUserAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellationToken)
        ?? throw new ApiClientException(response.StatusCode, "Die API hat keine gültige Antwort geliefert.");

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

            if (root.TryGetProperty("errors", out var errorsElement)
                && errorsElement.ValueKind == JsonValueKind.Object)
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

            if (root.TryGetProperty("detail", out var detail)
                && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                return (detail.GetString()!, field);
            }

            if (root.TryGetProperty("title", out var title)
                && !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return (title.GetString()!, field);
            }
        }
        catch (JsonException)
        {
            // Raw API content is intentionally not shown to users.
        }

        return ("Die Anfrage konnte nicht verarbeitet werden.", null);
    }
}
