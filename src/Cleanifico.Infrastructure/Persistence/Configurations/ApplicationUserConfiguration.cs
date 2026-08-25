using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName)
            .HasMaxLength(ApplicationUser.MaxNameLength)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(ApplicationUser.MaxNameLength)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(user => user.UpdatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex");

        builder.HasIndex(user => new { user.IsActive, user.LastName, user.FirstName })
            .HasDatabaseName("IX_AspNetUsers_Status_Name");
    }
}
