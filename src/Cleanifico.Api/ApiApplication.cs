using Cleanifico.Api.Endpoints;
using Cleanifico.Api.ErrorHandling;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.TimeTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Infrastructure;
using Cleanifico.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace Cleanifico.Api;

public static class ApiApplication
{
    public static WebApplication Build(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(ApiApplication).Assembly.GetName().Name
        });

        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<ICleaningTypeService, CleaningTypeService>();
        builder.Services.AddScoped<ITimeTypeService, TimeTypeService>();
        builder.Services.AddScoped<ITimeTypeInitializer, TimeTypeInitializer>();

        var connectionString = builder.Configuration.GetConnectionString(
            DependencyInjection.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The connection string 'ConnectionStrings:Cleanifico' is not configured.");
        }

        ConfigureSharedCookieDataProtection(builder.Services, builder.Configuration);
        builder.Services.AddCleanificoInfrastructure(connectionString, builder.Configuration);
        builder.Services.AddCleanificoApiAuthorization();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = OfficeAuthentication.CookieName;
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.LoginPath = "/api/auth/login";
            options.AccessDeniedPath = "/api/auth/access-denied";
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        });
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.UseExceptionHandler();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapAuthenticationEndpoints();
        app.MapUserAdministrationEndpoints();
        app.MapCleaningTypeEndpoints();
        app.MapTimeTypeEndpoints();

        return app;
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
