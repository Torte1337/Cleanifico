using Cleanifico.Application.Licensing;
using Cleanifico.Infrastructure.Licensing.Leases;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Licensing;

internal sealed class LocalLeaseLicenseService :
    ILicenseService,
    ILicenseActivationService,
    ILicenseRefreshService,
    IDisposable
{
    public const string BaseFeatureCode = "base";

    private readonly ILocalLicenseStore store;
    private readonly FergensHubLicensingClient client;
    private readonly LicenseLeaseVerifier verifier;
    private readonly TimeProvider timeProvider;
    private readonly SemaphoreSlim gate = new(1, 1);
    private string? degradedReason;

    public LocalLeaseLicenseService(
        ILocalLicenseStore store,
        FergensHubLicensingClient client,
        IOptions<LicensingOptions> options,
        TimeProvider timeProvider)
        : this(store, client, options, timeProvider, CleanificoLicenseTrustAnchors.Create())
    {
    }

    internal LocalLeaseLicenseService(
        ILocalLicenseStore store,
        FergensHubLicensingClient client,
        IOptions<LicensingOptions> options,
        TimeProvider timeProvider,
        IEnumerable<TrustedLicensePublicKey> trustedKeys)
    {
        this.store = store;
        this.client = client;
        this.timeProvider = timeProvider;
        if (!string.Equals(
                options.Value.ProductCode,
                LicensingOptions.CleanificoProductCode,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Licensing:ProductCode muss dem stabilen Cleanifico-Produktcode CLEANIFICO entsprechen.");
        }

        verifier = new LicenseLeaseVerifier(options.Value.ProductCode, trustedKeys);
    }

    public async Task<LicenseCheckResult> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            LocalLicenseLoadResult loaded = await EnsureStateAsync(cancellationToken);
            return ToCheckResult(loaded);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<LicenseOperationResult> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default)
    {
        if (!LicensingCredentialFormat.IsWellFormedLicenseKey(licenseKey))
        {
            return new(LicenseOperationStatus.InvalidLicenseKey);
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            LocalLicenseLoadResult loaded = await EnsureStateAsync(cancellationToken);
            if (!loaded.Succeeded)
            {
                return new(MapLocalFailure(loaded.Status));
            }

            LocalLicenseState current = loaded.State!;
            LicensingClientResult<FergensHubLicensingClient.ActivateLicenseResponse> response =
                await client.ActivateAsync(licenseKey, current.InstallationId, cancellationToken);
            if (!response.Succeeded)
            {
                LicenseOperationStatus failure = MapClientFailure(response.Error);
                degradedReason = failure.ToString();
                return new(failure);
            }

            FergensHubLicensingClient.ActivateLicenseResponse value = response.Value!;
            if (value.InstallationId != current.InstallationId
                || !LicensingCredentialFormat.IsWellFormedRefreshCredential(value.RefreshCredential)
                || !verifier.Verify(value.Lease, current.InstallationId).Succeeded)
            {
                degradedReason = LicenseOperationStatus.InvalidServerResponse.ToString();
                return new(LicenseOperationStatus.InvalidServerResponse);
            }

            var newState = new LocalLicenseState(
                LocalLicenseState.CurrentSchemaVersion,
                current.InstallationId,
                value.RefreshCredential,
                value.Lease,
                timeProvider.GetUtcNow().ToUniversalTime());
            if (!await TrySaveAsync(newState, cancellationToken))
            {
                return new(LicenseOperationStatus.LocalStateUnavailable);
            }

            degradedReason = null;
            return new(LicenseOperationStatus.Success);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<LicenseOperationResult> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            LocalLicenseLoadResult loaded = await EnsureStateAsync(cancellationToken);
            if (!loaded.Succeeded)
            {
                return new(MapLocalFailure(loaded.Status));
            }

            LocalLicenseState current = loaded.State!;
            if (current.RefreshCredential is null)
            {
                return new(LicenseOperationStatus.NotActivated);
            }

            LicensingClientResult<FergensHubLicensingClient.RefreshLicenseResponse> response =
                await client.RefreshAsync(
                    current.InstallationId,
                    current.RefreshCredential,
                    cancellationToken);
            if (!response.Succeeded)
            {
                LicenseOperationStatus failure = MapClientFailure(response.Error);
                degradedReason = failure.ToString();
                return new(failure);
            }

            SignedLicenseLease lease = response.Value!.Lease;
            if (!verifier.Verify(lease, current.InstallationId).Succeeded)
            {
                degradedReason = LicenseOperationStatus.InvalidLease.ToString();
                return new(LicenseOperationStatus.InvalidLease);
            }

            var newState = new LocalLicenseState(
                LocalLicenseState.CurrentSchemaVersion,
                current.InstallationId,
                current.RefreshCredential,
                lease,
                timeProvider.GetUtcNow().ToUniversalTime());
            if (!await TrySaveAsync(newState, cancellationToken))
            {
                return new(LicenseOperationStatus.LocalStateUnavailable);
            }

            degradedReason = null;
            return new(LicenseOperationStatus.Success);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();

    private async Task<LocalLicenseLoadResult> EnsureStateAsync(
        CancellationToken cancellationToken)
    {
        LocalLicenseLoadResult loaded = await store.LoadAsync(cancellationToken);
        if (loaded.Status != LocalLicenseLoadStatus.NotFound)
        {
            return loaded;
        }

        try
        {
            var state = new LocalLicenseState(
                LocalLicenseState.CurrentSchemaVersion,
                Guid.NewGuid());
            await store.SaveAsync(state, cancellationToken);
            return new(LocalLicenseLoadStatus.Success, state);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsLocalStateException(exception))
        {
            return new(LocalLicenseLoadStatus.Unavailable, null);
        }
    }

    private LicenseCheckResult ToCheckResult(LocalLicenseLoadResult loaded)
    {
        if (!loaded.Succeeded)
        {
            return new(LicenseStatus.Invalid, DegradedReason: loaded.Status.ToString());
        }

        LocalLicenseState state = loaded.State!;
        if (state.SignedLicenseLease is null)
        {
            return new(
                LicenseStatus.NotActivated,
                state.InstallationId,
                DegradedReason: degradedReason);
        }

        LicenseLeaseVerificationResult verification = verifier.Verify(
            state.SignedLicenseLease,
            state.InstallationId);
        if (!verification.Succeeded
            || !state.SignedLicenseLease.Payload.FeatureCodes.Contains(
                BaseFeatureCode,
                StringComparer.Ordinal))
        {
            return new(
                LicenseStatus.Invalid,
                state.InstallationId,
                DegradedReason: verification.Status.ToString());
        }

        LicenseLeasePayload payload = state.SignedLicenseLease.Payload;
        if (degradedReason is not null
            && degradedReason != LicenseOperationStatus.LicensingUnavailable.ToString())
        {
            return new(
                LicenseStatus.Invalid,
                state.InstallationId,
                payload.LicenseDisplayIdentifier,
                payload.ValidUntilUtc,
                payload.GraceUntilUtc,
                state.LastSuccessfulRefreshAtUtc,
                payload.FeatureCodes,
                degradedReason);
        }

        LicenseStatus status = LicenseLeaseTimeEvaluator.Evaluate(
            payload,
            timeProvider.GetUtcNow()) switch
        {
            LicenseLeaseTimeState.Valid => LicenseStatus.Valid,
            LicenseLeaseTimeState.Grace => LicenseStatus.Grace,
            LicenseLeaseTimeState.Expired => LicenseStatus.Expired,
            _ => LicenseStatus.Invalid
        };
        return new(
            status,
            state.InstallationId,
            payload.LicenseDisplayIdentifier,
            payload.ValidUntilUtc,
            payload.GraceUntilUtc,
            state.LastSuccessfulRefreshAtUtc,
            payload.FeatureCodes,
            degradedReason);
    }

    private async Task<bool> TrySaveAsync(
        LocalLicenseState state,
        CancellationToken cancellationToken)
    {
        try
        {
            await store.SaveAsync(state, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsLocalStateException(exception))
        {
            degradedReason = LicenseOperationStatus.LocalStateUnavailable.ToString();
            return false;
        }
    }

    private static LicenseOperationStatus MapClientFailure(LicensingClientError error) => error switch
    {
        LicensingClientError.InvalidCredential or LicensingClientError.InvalidLicense =>
            LicenseOperationStatus.InvalidLicense,
        LicensingClientError.LicenseSuspended => LicenseOperationStatus.LicenseSuspended,
        LicensingClientError.LicenseRevoked => LicenseOperationStatus.LicenseRevoked,
        LicensingClientError.LicenseExpired => LicenseOperationStatus.LicenseExpired,
        LicensingClientError.InstallationLimitReached => LicenseOperationStatus.InstallationLimitReached,
        LicensingClientError.InstallationRevoked => LicenseOperationStatus.InstallationRevoked,
        LicensingClientError.ConcurrencyConflict => LicenseOperationStatus.ConcurrencyConflict,
        LicensingClientError.RateLimited
            or LicensingClientError.ServiceUnavailable
            or LicensingClientError.Timeout
            or LicensingClientError.NetworkFailure => LicenseOperationStatus.LicensingUnavailable,
        _ => LicenseOperationStatus.InvalidServerResponse
    };

    private static LicenseOperationStatus MapLocalFailure(LocalLicenseLoadStatus status) =>
        status == LocalLicenseLoadStatus.Invalid
            ? LicenseOperationStatus.InvalidLocalState
            : LicenseOperationStatus.LocalStateUnavailable;

    private static bool IsLocalStateException(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or ArgumentException;
}

internal static class CleanificoLicenseTrustAnchors
{
    public static IEnumerable<TrustedLicensePublicKey> Create() =>
    [
        new(
            "fergenix-licensing-2026-08",
            LicenseLeaseConstants.Algorithm,
            "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEkJrRv4CBy5VLE8/k2h8c2bkbla0+IsuEdzkvz/L5kNNPs9KxlftUJSbMRjljf4iZ1zmYFzE0HWA2rtJJsMOqcA==")
    ];
}
