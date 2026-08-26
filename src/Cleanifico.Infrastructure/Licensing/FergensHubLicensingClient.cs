using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cleanifico.Application.Licensing;
using Cleanifico.Infrastructure.Licensing.Leases;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Licensing;

internal enum LicensingClientError
{
    None,
    InvalidRequest,
    InvalidCredential,
    InvalidLicense,
    LicenseSuspended,
    LicenseRevoked,
    LicenseExpired,
    InstallationLimitReached,
    InstallationRevoked,
    ConcurrencyConflict,
    RateLimited,
    ServiceUnavailable,
    Timeout,
    NetworkFailure,
    InvalidResponse
}

internal sealed record LicensingClientResult<T>(T? Value, LicensingClientError Error)
    where T : class
{
    public bool Succeeded => Error == LicensingClientError.None && Value is not null;
}

internal sealed class FergensHubLicensingClient(
    IHttpClientFactory httpClientFactory,
    IOptions<LicensingOptions> options)
{
    public const string HttpClientName = "FergensHubLicensing";
    public const string ActivatePath = "api/licensing/v1/activate";
    public const string RefreshPath = "api/licensing/v1/refresh";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public Task<LicensingClientResult<ActivateLicenseResponse>> ActivateAsync(
        string licenseKey,
        Guid installationId,
        CancellationToken cancellationToken)
    {
        var request = new ActivateLicenseRequest
        {
            LicenseKey = licenseKey,
            ProductCode = options.Value.ProductCode,
            InstallationId = installationId,
            ProductVersion = typeof(FergensHubLicensingClient).Assembly.GetName().Version?.ToString()
        };
        return SendAsync<ActivateLicenseRequest, ActivateLicenseResponse>(
            ActivatePath,
            request,
            cancellationToken);
    }

    public Task<LicensingClientResult<RefreshLicenseResponse>> RefreshAsync(
        Guid installationId,
        string refreshCredential,
        CancellationToken cancellationToken)
    {
        var request = new RefreshLicenseRequest
        {
            InstallationId = installationId,
            RefreshCredential = refreshCredential,
            ProductCode = options.Value.ProductCode,
            ProductVersion = typeof(FergensHubLicensingClient).Assembly.GetName().Version?.ToString()
        };
        return SendAsync<RefreshLicenseRequest, RefreshLicenseResponse>(
            RefreshPath,
            request,
            cancellationToken);
    }

    private async Task<LicensingClientResult<TResponse>> SendAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        Uri? baseUrl = options.Value.BaseUrl;
        if (baseUrl is not { IsAbsoluteUri: true }
            || !string.Equals(baseUrl.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(baseUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(baseUrl.UserInfo)
            || !string.IsNullOrEmpty(baseUrl.Query)
            || !string.IsNullOrEmpty(baseUrl.Fragment))
        {
            return new(null, LicensingClientError.ServiceUnavailable);
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient(HttpClientName);
            client.BaseAddress = baseUrl.AbsoluteUri.EndsWith('/')
                ? baseUrl
                : new Uri(baseUrl.AbsoluteUri + '/', UriKind.Absolute);
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                path,
                request,
                JsonOptions,
                cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                TResponse? value = await response.Content.ReadFromJsonAsync<TResponse>(
                    JsonOptions,
                    cancellationToken);
                return value is null
                    ? new(null, LicensingClientError.InvalidResponse)
                    : new(value, LicensingClientError.None);
            }

            LicensingErrorResponse? error = await response.Content.ReadFromJsonAsync<LicensingErrorResponse>(
                JsonOptions,
                cancellationToken);
            return new(null, MapError(error?.Code));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            return new(null, LicensingClientError.Timeout);
        }
        catch (HttpRequestException)
        {
            return new(null, LicensingClientError.NetworkFailure);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return new(null, LicensingClientError.InvalidResponse);
        }
    }

    private static LicensingClientError MapError(string? code) => code switch
    {
        "invalid_request" => LicensingClientError.InvalidRequest,
        "invalid_credential" => LicensingClientError.InvalidCredential,
        "invalid_license" => LicensingClientError.InvalidLicense,
        "license_suspended" => LicensingClientError.LicenseSuspended,
        "license_revoked" => LicensingClientError.LicenseRevoked,
        "license_expired" => LicensingClientError.LicenseExpired,
        "installation_limit_reached" => LicensingClientError.InstallationLimitReached,
        "installation_revoked" => LicensingClientError.InstallationRevoked,
        "concurrency_conflict" => LicensingClientError.ConcurrencyConflict,
        "rate_limited" => LicensingClientError.RateLimited,
        "service_unavailable" => LicensingClientError.ServiceUnavailable,
        _ => LicensingClientError.InvalidResponse
    };

    private sealed class ActivateLicenseRequest
    {
        public string? LicenseKey { get; init; }
        public string? ProductCode { get; init; }
        public Guid InstallationId { get; init; }
        public string? ProductVersion { get; init; }

        public override string ToString() =>
            $"{nameof(ActivateLicenseRequest)} {{ LicenseKey = [REDACTED], ProductCode = {ProductCode}, InstallationId = {InstallationId}, ProductVersion = {ProductVersion} }}";
    }

    internal sealed class ActivateLicenseResponse
    {
        [JsonConstructor]
        public ActivateLicenseResponse(
            Guid installationId,
            string refreshCredential,
            SignedLicenseLease lease)
        {
            InstallationId = installationId;
            RefreshCredential = refreshCredential;
            Lease = lease;
        }

        public Guid InstallationId { get; }
        public string RefreshCredential { get; }
        public SignedLicenseLease Lease { get; }

        public override string ToString() =>
            $"{nameof(ActivateLicenseResponse)} {{ InstallationId = {InstallationId}, RefreshCredential = [REDACTED], LeaseId = {Lease.Payload.LeaseId} }}";
    }

    private sealed class RefreshLicenseRequest
    {
        public Guid InstallationId { get; init; }
        public string? RefreshCredential { get; init; }
        public string? ProductCode { get; init; }
        public string? ProductVersion { get; init; }

        public override string ToString() =>
            $"{nameof(RefreshLicenseRequest)} {{ InstallationId = {InstallationId}, RefreshCredential = [REDACTED], ProductCode = {ProductCode}, ProductVersion = {ProductVersion} }}";
    }

    internal sealed class RefreshLicenseResponse
    {
        [JsonConstructor]
        public RefreshLicenseResponse(SignedLicenseLease lease) => Lease = lease;

        public SignedLicenseLease Lease { get; }
    }

    private sealed record LicensingErrorResponse(string Code, string Message);
}
