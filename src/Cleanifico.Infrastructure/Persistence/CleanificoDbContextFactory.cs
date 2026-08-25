using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cleanifico.Infrastructure.Persistence;

public sealed class CleanificoDbContextFactory : IDesignTimeDbContextFactory<CleanificoDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Port=3306;Database=cleanifico_design;User=__design_time__;Password=__not_used__";

    public CleanificoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CleanificoDbContext>()
            .UseMySql(
                DesignTimeConnectionString,
                DependencyInjection.DatabaseServerVersion,
                mySql => mySql.MigrationsAssembly(typeof(CleanificoDbContext).Assembly.FullName))
            .Options;

        return new CleanificoDbContext(options);
    }
}
