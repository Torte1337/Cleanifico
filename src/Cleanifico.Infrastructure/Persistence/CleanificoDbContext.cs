using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Persistence;

public sealed class CleanificoDbContext(DbContextOptions<CleanificoDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<CleaningType> CleaningTypes => Set<CleaningType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanificoDbContext).Assembly);
    }
}
