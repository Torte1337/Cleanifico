using Cleanifico.Domain.CleaningTypes;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Persistence;

public sealed class CleanificoDbContext(DbContextOptions<CleanificoDbContext> options)
    : DbContext(options)
{
    public DbSet<CleaningType> CleaningTypes => Set<CleaningType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanificoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
