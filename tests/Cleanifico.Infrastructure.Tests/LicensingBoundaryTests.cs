using Cleanifico.Application.Licensing;
using Cleanifico.Infrastructure.Licensing;

namespace Cleanifico.Infrastructure.Tests;

public sealed class LicensingBoundaryTests
{
    [Fact]
    public async Task MissingExternalFergensHubContract_FailsClosed()
    {
        var service = new UnavailableFergensHubLicenseService();

        var result = await service.CheckAsync();

        Assert.Equal(LicenseStatus.Unavailable, result.Status);
        Assert.False(result.IsValid);
    }
}
