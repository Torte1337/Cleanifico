using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Contracts.Licensing;
using Cleanifico.Web.Authentication;

namespace Cleanifico.Web.Licensing;

public sealed class OfficeLicenseService(
    IHttpClientFactory httpClientFactory,
    IOfficeApiRequestAuthenticator requestAuthenticator,
    TimeProvider timeProvider,
    ILogger<OfficeLicenseService> logger) : IOfficeLicenseService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    private LicenseStatusResponse? cachedStatus;
    private DateTimeOffset cacheExpiresAt;

    public async Task<LicenseStatusResponse> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        if (!forceRefresh && cachedStatus is not null && now < cacheExpiresAt)
        {
            return cachedStatus;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/license/status");
            await requestAuthenticator.ApplyAsync(request, cancellationToken);
            request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");
            using var response = await httpClientFactory.CreateClient("OfficeApi")
                .SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Cleanifico license status request failed with status {StatusCode}.",
                    (int)response.StatusCode);
                return Cache(Unavailable(), now);
            }

            var status = await response.Content.ReadFromJsonAsync<LicenseStatusResponse>(
                cancellationToken: cancellationToken);
            return Cache(status is not null && IsKnown(status.Status) ? status : Unavailable(), now);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "Cleanifico license status could not be obtained.");
            return Cache(Unavailable(), now);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Cleanifico license status request timed out.");
            return Cache(Unavailable(), now);
        }
    }

    public Task<LicenseOperationResponse> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default) =>
        SendOperationAsync(
            "api/license/activate",
            new ActivateLicenseRequest { LicenseKey = licenseKey },
            cancellationToken);

    public Task<LicenseOperationResponse> RefreshAsync(
        CancellationToken cancellationToken = default) =>
        SendOperationAsync<object?>("api/license/refresh", null, cancellationToken);

    private LicenseStatusResponse Cache(LicenseStatusResponse status, DateTimeOffset now)
    {
        cachedStatus = status;
        cacheExpiresAt = now.Add(CacheDuration);
        return status;
    }

    private static bool IsKnown(string status) => status is
        LicenseStatusCodes.NotActivated
        or LicenseStatusCodes.Valid
        or LicenseStatusCodes.Grace
        or LicenseStatusCodes.Expired
        or LicenseStatusCodes.Invalid;

    private static LicenseStatusResponse Unavailable() => new(
        LicenseStatusCodes.Invalid,
        false,
        "Der Lizenzstatus konnte nicht sicher ermittelt werden.",
        Guid.Empty,
        null,
        null,
        null,
        null,
        [],
        "ApiUnavailable");

    private async Task<LicenseOperationResponse> SendOperationAsync<TRequest>(
        string path,
        TRequest requestBody,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = requestBody is null
                    ? null
                    : JsonContent.Create(requestBody)
            };
            await requestAuthenticator.ApplyAsync(request, cancellationToken);
            request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");
            using HttpResponseMessage response = await httpClientFactory.CreateClient("OfficeApi")
                .SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return OperationUnavailable();
            }

            LicenseOperationResponse? result = await response.Content
                .ReadFromJsonAsync<LicenseOperationResponse>(cancellationToken: cancellationToken);
            cachedStatus = null;
            return result ?? OperationUnavailable();
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "Cleanifico license operation could not be completed.");
            return OperationUnavailable();
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Cleanifico license operation timed out.");
            return OperationUnavailable();
        }
    }

    private static LicenseOperationResponse OperationUnavailable() => new(
        "LicensingUnavailable",
        false,
        "Die Lizenzaktion konnte nicht ausgeführt werden.");
}
