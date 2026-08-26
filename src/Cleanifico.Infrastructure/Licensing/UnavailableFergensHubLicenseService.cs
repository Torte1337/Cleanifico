using Cleanifico.Application.Licensing;

namespace Cleanifico.Infrastructure.Licensing;

public sealed class UnavailableFergensHubLicenseService : ILicenseService
{
    public Task<LicenseCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new LicenseCheckResult(LicenseStatus.Unavailable));
    }
}
