using Cleanifico.Contracts.Licensing;

namespace Cleanifico.Web.Licensing;

public interface IOfficeLicenseService
{
    Task<LicenseStatusResponse> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<LicenseOperationResponse> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default);

    Task<LicenseOperationResponse> RefreshAsync(
        CancellationToken cancellationToken = default);
}
