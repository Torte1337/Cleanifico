using Cleanifico.Application.CleaningObjects;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.Customers;
using Cleanifico.Application.Employees;
using Cleanifico.Application.Licensing;
using Cleanifico.Application.Security;
using Cleanifico.Application.TimeTypes;
using Cleanifico.Infrastructure.Persistence;
using Cleanifico.Infrastructure.Licensing;
using Cleanifico.Infrastructure.Persistence.Repositories;
using Cleanifico.Infrastructure.Security.Bootstrap;
using Cleanifico.Infrastructure.Security.Identity;
using Cleanifico.Infrastructure.TimeTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cleanifico.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Cleanifico";

    public static readonly MySqlServerVersion DatabaseServerVersion =
        new(new Version(8, 4, 0));

    public static IServiceCollection AddCleanificoInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<CleanificoDbContext>(options =>
            options.UseMySql(
                connectionString,
                DatabaseServerVersion,
                mySql =>
                {
                    mySql.MigrationsAssembly(typeof(CleanificoDbContext).Assembly.FullName);
                    mySql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

        services.AddScoped<ICleaningTypeRepository, EfCleaningTypeRepository>();
        services.AddScoped<ICleaningObjectRepository, EfCleaningObjectRepository>();
        services.AddScoped<ICustomerRepository, EfCustomerRepository>();
        services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
        services.AddScoped<ITimeTypeRepository, EfTimeTypeRepository>();
        services.AddOptions<LicensingOptions>()
            .Bind(configuration.GetSection(LicensingOptions.SectionName))
            .Validate(
                options => options.RequestTimeout > TimeSpan.Zero
                    && options.RequestTimeout <= TimeSpan.FromMinutes(5),
                "Licensing:RequestTimeout muss größer null und höchstens fünf Minuten sein.")
            .Validate(
                options => options.RefreshInterval >= LicensingOptions.MinimumRefreshInterval
                    && options.RefreshInterval <= LicensingOptions.MaximumRefreshInterval,
                "Licensing:RefreshInterval muss zwischen einer Stunde und 30 Tagen liegen.")
            .ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ILocalLicenseStore, FileLocalLicenseStore>();
        services.AddHttpClient(FergensHubLicensingClient.HttpClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LicensingOptions>>();
            client.Timeout = options.Value.RequestTimeout;
        });
        services.AddSingleton<FergensHubLicensingClient>();
        services.AddSingleton<LocalLeaseLicenseService>();
        services.AddSingleton<ILicenseService>(provider =>
            provider.GetRequiredService<LocalLeaseLicenseService>());
        services.AddSingleton<ILicenseActivationService>(provider =>
            provider.GetRequiredService<LocalLeaseLicenseService>());
        services.AddSingleton<ILicenseRefreshService>(provider =>
            provider.GetRequiredService<LocalLeaseLicenseService>());
        services.AddHostedService<LicenseRefreshBackgroundService>();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentitySecurityDefaults.Configure)
            .AddEntityFrameworkStores<CleanificoDbContext>()
            .AddErrorDescriber<GermanIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(2));
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.Zero);

        services.AddSingleton<OwnerProtectionGate>();
        services.AddScoped<IUserAdministrationService, IdentityUserAdministrationService>();
        services.AddScoped<IUserAuthenticationService, IdentityUserAuthenticationService>();
        services.AddScoped<IRoleBootstrapper, IdentityRoleBootstrapper>();

        services.Configure<SecurityBootstrapOptions>(
            configuration.GetSection(SecurityBootstrapOptions.SectionName));
        services.AddHostedService<IdentityBootstrapHostedService>();

        services.Configure<TimeTypeBootstrapOptions>(
            configuration.GetSection(TimeTypeBootstrapOptions.SectionName));
        services.AddHostedService<TimeTypeBootstrapHostedService>();

        return services;
    }
}
