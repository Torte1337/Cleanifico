using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Cleanifico.Contracts.Security;
using Cleanifico.Contracts.Licensing;
using Cleanifico.Web;
using Cleanifico.Web.Licensing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Web.Tests;

public sealed class OfficeWebSecurityTests
{
    [Fact]
    public async Task LoginPage_IsAvailableAnonymously()
    {
        await using var host = await OfficeWebTestHost.StartAsync(null, anonymous: true);

        using var response = await host.Client.GetAsync("/login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Anmelden", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Registrieren", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Dashboard_IsNotRenderedForAnonymousUser()
    {
        await using var host = await OfficeWebTestHost.StartAsync(null, anonymous: true);

        using var response = await host.Client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Redirect
            || !html.Contains("Betriebssoftware für Gebäudereinigungsunternehmen", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(SecurityRoles.Owner)]
    [InlineData(SecurityRoles.Administrator)]
    [InlineData(SecurityRoles.Dispatcher)]
    [InlineData(SecurityRoles.ObjectManager)]
    public async Task OfficeRoles_CanOpenDashboard(string role)
    {
        await using var host = await OfficeWebTestHost.StartAsync(role, anonymous: false);

        using var response = await host.Client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Betriebssoftware für Gebäudereinigungsunternehmen", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(SecurityRoles.Owner, true)]
    [InlineData(SecurityRoles.Administrator, true)]
    [InlineData(SecurityRoles.Dispatcher, false)]
    [InlineData(SecurityRoles.ObjectManager, false)]
    [InlineData(SecurityRoles.Employee, false)]
    public async Task UserAdministration_RequiresAdministrativeRole(string role, bool allowed)
    {
        await using var host = await OfficeWebTestHost.StartAsync(role, anonymous: false);

        using var response = await host.Client.GetAsync("/administration/benutzer");
        var html = await response.Content.ReadAsStringAsync();

        if (allowed)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Verwalten Sie tenantlokale Zugänge", html, StringComparison.Ordinal);
        }
        else
        {
            Assert.True(
                response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect
                || !html.Contains("Verwalten Sie tenantlokale Zugänge", StringComparison.Ordinal));
        }
    }

    [Theory]
    [InlineData(SecurityRoles.Owner, true)]
    [InlineData(SecurityRoles.Administrator, true)]
    [InlineData(SecurityRoles.Dispatcher, false)]
    [InlineData(SecurityRoles.ObjectManager, false)]
    public async Task OfficeRoles_CanOpenTimeTypesWithRoleAppropriateActions(
        string role,
        bool canManage)
    {
        await using var host = await OfficeWebTestHost.StartAsync(role, anonymous: false);

        using var response = await host.Client.GetAsync("/zeittypen");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Konfigurieren Sie die Zeit- und Abwesenheitsarten", html, StringComparison.Ordinal);
        Assert.Equal(canManage, html.Contains("Zeittyp anlegen", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Employee_CannotOpenTimeTypes()
    {
        await using var host = await OfficeWebTestHost.StartAsync(SecurityRoles.Employee, anonymous: false);

        using var response = await host.Client.GetAsync("/zeittypen");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect
            || !html.Contains("Konfigurieren Sie die Zeit- und Abwesenheitsarten", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(SecurityRoles.Owner, true)]
    [InlineData(SecurityRoles.Administrator, true)]
    [InlineData(SecurityRoles.Dispatcher, false)]
    [InlineData(SecurityRoles.ObjectManager, false)]
    public async Task OfficeRoles_CanOpenCustomersWithRoleAppropriateActions(
        string role,
        bool canManage)
    {
        await using var host = await OfficeWebTestHost.StartAsync(role, anonymous: false);

        using var response = await host.Client.GetAsync("/kunden");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Verwalten Sie Auftraggeber, Ansprechpartner", html, StringComparison.Ordinal);
        Assert.Equal(canManage, html.Contains("Kunde anlegen", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Employee_CannotOpenCustomers()
    {
        await using var host = await OfficeWebTestHost.StartAsync(SecurityRoles.Employee, anonymous: false);

        using var response = await host.Client.GetAsync("/kunden");
        var html = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect
                || !html.Contains("Verwalten Sie Auftraggeber, Ansprechpartner", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(SecurityRoles.Owner, true)]
    [InlineData(SecurityRoles.Administrator, true)]
    [InlineData(SecurityRoles.Dispatcher, false)]
    [InlineData(SecurityRoles.ObjectManager, false)]
    public async Task OfficeRoles_CanOpenObjectsWithRoleAppropriateActions(string role, bool canManage)
    {
        await using var host = await OfficeWebTestHost.StartAsync(role, anonymous: false);
        using var response = await host.Client.GetAsync("/objekte");
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Verwalten Sie Reinigungsobjekte", html, StringComparison.Ordinal);
        Assert.Equal(canManage, html.Contains("Objekt anlegen", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Employee_CannotOpenObjects()
    {
        await using var host = await OfficeWebTestHost.StartAsync(SecurityRoles.Employee, anonymous: false);
        using var response = await host.Client.GetAsync("/objekte");
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect
            || !html.Contains("Verwalten Sie Reinigungsobjekte", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(LicenseStatusCodes.NotActivated, "Installation nicht aktiviert")]
    [InlineData(LicenseStatusCodes.Expired, "Lizenz abgelaufen")]
    [InlineData(LicenseStatusCodes.Invalid, "Lizenzzustand ung")]
    public async Task LicensePage_ShowsControlledStatus(string licenseStatus, string expectedText)
    {
        await using var host = await OfficeWebTestHost.StartAsync(
            SecurityRoles.Owner,
            anonymous: false,
            licenseStatus: licenseStatus);

        using var response = await host.Client.GetAsync("/lizenz");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedText, html, StringComparison.Ordinal);
        Assert.DoesNotContain("http://", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidLicense_DoesNotRenderBusinessPage()
    {
        await using var host = await OfficeWebTestHost.StartAsync(
            SecurityRoles.Owner,
            anonymous: false,
            licenseStatus: LicenseStatusCodes.Expired);

        using var response = await host.Client.GetAsync("/kunden");
        var html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Verwalten Sie Auftraggeber, Ansprechpartner", html, StringComparison.Ordinal);
    }
}

internal sealed class OfficeWebTestHost : IAsyncDisposable
{
    private readonly WebApplication application;

    private OfficeWebTestHost(WebApplication application, HttpClient client)
    {
        this.application = application;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<OfficeWebTestHost> StartAsync(
        string? role,
        bool anonymous,
        string licenseStatus = LicenseStatusCodes.Valid)
    {
        var application = OfficeWebApplication.Build(
            [
                "--environment", "Testing",
                "--CleanificoApi:BaseUrl", "http://127.0.0.1:65534",
                "--Authentication:DataProtectionKeysPath",
                Path.Combine(Path.GetTempPath(), "cleanifico-web-tests-keys")
            ],
            services =>
            {
                services.AddSingleton(new WebTestIdentity(role, anonymous));
                services.AddSingleton<IOfficeLicenseService>(new FakeOfficeLicenseService(licenseStatus));
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = WebTestAuthenticationHandler.SchemeName;
                        options.DefaultChallengeScheme = WebTestAuthenticationHandler.SchemeName;
                        options.DefaultForbidScheme = WebTestAuthenticationHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, WebTestAuthenticationHandler>(
                        WebTestAuthenticationHandler.SchemeName,
                        _ => { });
            });

        application.Urls.Add("http://127.0.0.1:0");
        await application.StartAsync();
        var server = application.Services.GetRequiredService<IServer>();
        var address = Assert.Single(
            server.Features.Get<IServerAddressesFeature>()?.Addresses ?? []);
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        return new OfficeWebTestHost(
            application,
            new HttpClient(handler) { BaseAddress = new Uri(address) });
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.StopAsync();
        await application.DisposeAsync();
    }
}

internal sealed class FakeOfficeLicenseService(string status) : IOfficeLicenseService
{
    public Task<LicenseStatusResponse> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseStatusResponse(
            status,
            status is LicenseStatusCodes.Valid or LicenseStatusCodes.Grace,
            status switch
            {
                LicenseStatusCodes.Valid => "Die Cleanifico-Lizenz ist gültig.",
                LicenseStatusCodes.Grace => "Die Cleanifico-Lizenz befindet sich im Offline-Toleranzzeitraum.",
                LicenseStatusCodes.NotActivated => "Diese Cleanifico-Installation wurde noch nicht aktiviert.",
                LicenseStatusCodes.Expired => "Die Cleanifico-Lizenz ist abgelaufen.",
                _ => "Der lokale Cleanifico-Lizenzzustand ist ungültig."
            },
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            null,
            null,
            null,
            null,
            [],
            null));

    public Task<LicenseOperationResponse> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseOperationResponse("Success", true, "Aktiviert."));

    public Task<LicenseOperationResponse> RefreshAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseOperationResponse("Success", true, "Aktualisiert."));
}

internal sealed record WebTestIdentity(string? Role, bool Anonymous);

internal sealed class WebTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    WebTestIdentity identity) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Cleanifico.Web.Tests";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (identity.Anonymous)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "web-test@cleanifico.test")
        };
        if (identity.Role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, identity.Role));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
