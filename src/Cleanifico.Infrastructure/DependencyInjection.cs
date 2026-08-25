using Cleanifico.Application.CleaningTypes;
using Cleanifico.Infrastructure.Persistence;
using Cleanifico.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cleanifico.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Cleanifico";

    public static readonly MySqlServerVersion DatabaseServerVersion =
        new(new Version(8, 4, 0));

    public static IServiceCollection AddCleanificoInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

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

        return services;
    }
}
