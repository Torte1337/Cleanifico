using Cleanifico.Domain.CleaningTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class CleaningTypeConfiguration : IEntityTypeConfiguration<CleaningType>
{
    public void Configure(EntityTypeBuilder<CleaningType> builder)
    {
        builder.ToTable("CleaningTypes");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(cleaningType => cleaningType.Id);

        builder.Property(cleaningType => cleaningType.Id)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .ValueGeneratedNever();

        builder.Property(cleaningType => cleaningType.Name)
            .HasMaxLength(CleaningType.MaxNameLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();

        builder.Property(cleaningType => cleaningType.Code)
            .HasMaxLength(CleaningType.MaxCodeLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();

        builder.Property(cleaningType => cleaningType.Description)
            .HasMaxLength(CleaningType.MaxDescriptionLength);

        builder.Property(cleaningType => cleaningType.IsActive)
            .IsRequired();

        builder.Property(cleaningType => cleaningType.SortOrder)
            .IsRequired();

        builder.Property(cleaningType => cleaningType.CreatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(cleaningType => cleaningType.UpdatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.HasIndex(cleaningType => cleaningType.Name)
            .IsUnique()
            .HasDatabaseName("UX_CleaningTypes_Name");

        builder.HasIndex(cleaningType => cleaningType.Code)
            .IsUnique()
            .HasDatabaseName("UX_CleaningTypes_Code");

        builder.HasIndex(cleaningType => new
            {
                cleaningType.IsActive,
                cleaningType.SortOrder,
                cleaningType.Name
            })
            .HasDatabaseName("IX_CleaningTypes_Status_SortOrder_Name");
    }
}
