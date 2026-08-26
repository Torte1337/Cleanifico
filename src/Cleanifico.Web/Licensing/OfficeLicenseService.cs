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

    private LicenseStatusResponse Cache(LicenseStatusResponse status, DateTimeOffset now)
    {
        cachedStatus = status;
        cacheExpiresAt = now.Add(CacheDuration);
        return status;
    }

    private static bool IsKnown(string status) => status is
        LicenseStatusCodes.Active
        or LicenseStatusCodes.Inactive
        or LicenseStatusCodes.NotFound
        or LicenseStatusCodes.Unavailable;

    private static LicenseStatusResponse Unavailable() => new(
        LicenseStatusCodes.Unavailable,
        false,
        "Die Lizenzprüfung ist derzeit nicht möglich.");
}
