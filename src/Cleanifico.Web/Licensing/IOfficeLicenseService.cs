using Cleanifico.Contracts.Licensing;

namespace Cleanifico.Web.Licensing;

public interface IOfficeLicenseService
{
    Task<LicenseStatusResponse> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);
}
