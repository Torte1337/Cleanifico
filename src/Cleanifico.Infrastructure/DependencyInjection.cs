using Cleanifico.Application.CleaningObjects;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.Customers;
using Cleanifico.Application.Security;
using Cleanifico.Application.TimeTypes;
using Cleanifico.Infrastructure.Persistence;
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
        services.AddScoped<ITimeTypeRepository, EfTimeTypeRepository>();

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
