namespace Cleanifico.Application.Licensing;

public enum LicenseOperationStatus
{
    Success,
    InvalidLicenseKey,
    NotActivated,
    LicensingUnavailable,
    InvalidServerResponse,
    InvalidLease,
    InvalidLocalState,
    LocalStateUnavailable,
    InvalidLicense,
    LicenseSuspended,
    LicenseRevoked,
    LicenseExpired,
    InstallationLimitReached,
    InstallationRevoked,
    ConcurrencyConflict
}

public sealed record LicenseOperationResult(LicenseOperationStatus Status)
{
    public bool Succeeded => Status == LicenseOperationStatus.Success;
}

public interface ILicenseActivationService
{
    Task<LicenseOperationResult> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default);
}

public interface ILicenseRefreshService
{
    Task<LicenseOperationResult> RefreshAsync(
        CancellationToken cancellationToken = default);
}
