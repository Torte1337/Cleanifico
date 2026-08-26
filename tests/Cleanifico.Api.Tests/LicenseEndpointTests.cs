using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Application.Licensing;
using Cleanifico.Contracts.Licensing;
using Cleanifico.Contracts.Security;

namespace Cleanifico.Api.Tests;

public sealed class LicenseEndpointTests
{
    [Fact]
    public async Task ActiveLicense_AllowsBusinessAccess()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.Active);

        using var response = await host.Client.GetAsync("/api/cleaning-types");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(LicenseStatus.Inactive, "Inactive")]
    [InlineData(LicenseStatus.NotFound, "NotFound")]
    [InlineData(LicenseStatus.Unavailable, "Unavailable")]
    public async Task InvalidLicense_BlocksBusinessAccessWithControlledProblem(
        LicenseStatus status,
        string expectedStatus)
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(status);

        using var response = await host.Client.GetAsync("/api/cleaning-types");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(expectedStatus, problem.RootElement.GetProperty("licenseStatus").GetString());
        Assert.DoesNotContain(
            "http",
            problem.RootElement.GetProperty("detail").GetString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/cleaning-types")]
    [InlineData("/api/time-types")]
    [InlineData("/api/customers")]
    [InlineData("/api/objects")]
    public async Task UnavailableLicense_BlocksEveryExistingBusinessApi(string path)
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.Unavailable);

        using var response = await host.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousBusinessAccess_RemainsUnauthorizedWhenLicenseIsInvalid()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.Inactive,
            role: null,
            anonymous: true);

        using var response = await host.Client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MissingRole_RemainsForbiddenWhenLicenseIsActive()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.Active,
            SecurityRoles.Employee);

        using var response = await host.Client.GetAsync("/api/objects");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnavailableProvider_ReturnsControlledStatusForOffice()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.Unavailable);

        var result = await host.Client.GetFromJsonAsync<LicenseStatusResponse>("/api/license/status");

        Assert.NotNull(result);
        Assert.Equal(LicenseStatusCodes.Unavailable, result.Status);
        Assert.False(result.IsValid);
        Assert.Equal("Die Lizenzprüfung ist derzeit nicht möglich.", result.Message);
    }

    [Fact]
    public async Task Health_RemainsAvailableWhenLicenseCannotBeChecked()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.Unavailable,
            role: null,
            anonymous: true);

        using var response = await host.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
