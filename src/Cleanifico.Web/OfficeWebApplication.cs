using Cleanifico.Contracts.Security;
using Cleanifico.Web.ApiClients;
using Cleanifico.Web.Authentication;
using Cleanifico.Web.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace Cleanifico.Web;

public static class OfficeWebApplication
{
    public static WebApplication Build(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(OfficeWebApplication).Assembly.GetName().Name
        });

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton(TimeProvider.System);

        var cleanificoApiBaseUrl = builder.Configuration["CleanificoApi:BaseUrl"];
        if (!Uri.TryCreate(cleanificoApiBaseUrl, UriKind.Absolute, out var cleanificoApiUri))
        {
            throw new InvalidOperationException(
                "The setting 'CleanificoApi:BaseUrl' must contain an absolute URI.");
        }

        ConfigureSharedCookieDataProtection(builder.Services, builder.Configuration);
        ConfigureAuthentication(builder.Services);
        ConfigureAuthorization(builder.Services);

        builder.Services.AddTransient<OfficeApiCookieHandler>();
        builder.Services.AddScoped<IOfficeApiRequestAuthenticator, OfficeApiRequestAuthenticator>();
        builder.Services.AddHttpClient("OfficeApi", client => client.BaseAddress = cleanificoApiUri)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false
            })
            .AddHttpMessageHandler<OfficeApiCookieHandler>();
        builder.Services.AddHttpClient<ICleaningTypesApiClient, CleaningTypesApiClient>(client =>
                client.BaseAddress = cleanificoApiUri)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
        builder.Services.AddHttpClient<ITimeTypesApiClient, TimeTypesApiClient>(client =>
                client.BaseAddress = cleanificoApiUri)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
        builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>(client =>
                client.BaseAddress = cleanificoApiUri)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        builder.Services.AddScoped<OfficeCookieEvents>();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapOfficeAuthenticationEndpoints();
        app.MapStaticAssets().AllowAnonymous();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OfficeAuthentication.CookieScheme;
                options.DefaultChallengeScheme = OfficeAuthentication.CookieScheme;
                options.DefaultSignInScheme = OfficeAuthentication.CookieScheme;
            })
            .AddCookie(OfficeAuthentication.CookieScheme, options =>
            {
                options.Cookie.Name = OfficeAuthentication.CookieName;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/zugriff-verweigert";
                options.EventsType = typeof(OfficeCookieEvents);
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(SecurityPolicies.OfficeAccess, policy =>
                policy.RequireRole([.. SecurityRoles.Office]))
            .AddPolicy(SecurityPolicies.ViewCleaningTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Office]))
            .AddPolicy(SecurityPolicies.ManageCleaningTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]))
            .AddPolicy(SecurityPolicies.ViewTimeTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Office]))
            .AddPolicy(SecurityPolicies.ManageTimeTypes, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]))
            .AddPolicy(SecurityPolicies.ManageUsers, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]))
            .AddPolicy(SecurityPolicies.ManageRoles, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]))
            .AddPolicy(SecurityPolicies.AdministrationAccess, policy =>
                policy.RequireRole([.. SecurityRoles.Administrators]));
    }

    private static void ConfigureSharedCookieDataProtection(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var configuredPath = configuration[OfficeAuthentication.DataProtectionKeysPathConfiguration];
        var keyPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Cleanifico",
                "DataProtectionKeys")
            : Path.GetFullPath(configuredPath);

        Directory.CreateDirectory(keyPath);
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
            .SetApplicationName(OfficeAuthentication.DataProtectionApplicationName);
    }
}
