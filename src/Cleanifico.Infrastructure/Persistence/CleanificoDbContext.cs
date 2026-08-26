using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Domain.Customers;
using Cleanifico.Domain.TimeTypes;
using Cleanifico.Infrastructure.Persistence.Initialization;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Persistence;

public sealed class CleanificoDbContext(DbContextOptions<CleanificoDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<CleaningObject> CleaningObjects => Set<CleaningObject>();

    public DbSet<CleaningType> CleaningTypes => Set<CleaningType>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<TimeType> TimeTypes => Set<TimeType>();

    public DbSet<DataInitializationMarker> DataInitializationMarkers =>
        Set<DataInitializationMarker>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanificoDbContext).Assembly);
    }
}
