using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cleanifico.Application.Licensing;
using Cleanifico.Contracts.Licensing;
using Cleanifico.Contracts.Security;

namespace Cleanifico.Api.Tests;

public sealed class LicenseEndpointTests
{
    [Theory]
    [InlineData(LicenseStatus.Valid)]
    [InlineData(LicenseStatus.Grace)]
    public async Task OperationalLease_AllowsBusinessAccess(LicenseStatus status)
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(status);

        using var response = await host.Client.GetAsync("/api/cleaning-types");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LicenseActivation_RequiresAdministratorRole()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.NotActivated,
            SecurityRoles.Dispatcher);

        using var response = await host.Client.PostAsJsonAsync(
            "/api/license/activate",
            new ActivateLicenseRequest { LicenseKey = "flk1_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopq" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanTriggerLicenseRefreshWithoutValidBusinessLicense()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.NotActivated);

        using var response = await host.Client.PostAsync("/api/license/refresh", null);
        var result = await response.Content.ReadFromJsonAsync<LicenseOperationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(LicenseStatus.NotActivated, "NotActivated")]
    [InlineData(LicenseStatus.Expired, "Expired")]
    [InlineData(LicenseStatus.Invalid, "Invalid")]
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
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.Invalid);

        using var response = await host.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousBusinessAccess_RemainsUnauthorizedWhenLicenseIsInvalid()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.NotActivated,
            role: null,
            anonymous: true);

        using var response = await host.Client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MissingRole_RemainsForbiddenWhenLicenseIsActive()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.Valid,
            SecurityRoles.Employee);

        using var response = await host.Client.GetAsync("/api/objects");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnavailableProvider_ReturnsControlledStatusForOffice()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(LicenseStatus.Invalid);

        var result = await host.Client.GetFromJsonAsync<LicenseStatusResponse>("/api/license/status");

        Assert.NotNull(result);
        Assert.Equal(LicenseStatusCodes.Invalid, result.Status);
        Assert.False(result.IsValid);
        Assert.Equal("Der lokale Cleanifico-Lizenzzustand ist ungültig.", result.Message);
    }

    [Fact]
    public async Task Health_RemainsAvailableWhenLicenseCannotBeChecked()
    {
        await using var host = await ApiTestHost.StartWithLicenseAsync(
            LicenseStatus.Invalid,
            role: null,
            anonymous: true);

        using var response = await host.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
