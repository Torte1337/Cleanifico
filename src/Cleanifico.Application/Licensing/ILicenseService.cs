namespace Cleanifico.Application.Licensing;

public interface ILicenseService
{
    Task<LicenseCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}
