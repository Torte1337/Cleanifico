using Cleanifico.Domain.TimeTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class TimeTypeConfiguration : IEntityTypeConfiguration<TimeType>
{
    public void Configure(EntityTypeBuilder<TimeType> builder)
    {
        builder.ToTable("TimeTypes");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(timeType => timeType.Id);

        builder.Property(timeType => timeType.Id)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .ValueGeneratedNever();

        builder.Property(timeType => timeType.Name)
            .HasMaxLength(TimeType.MaxNameLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();

        builder.Property(timeType => timeType.Code)
            .HasMaxLength(TimeType.MaxCodeLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();

        builder.Property(timeType => timeType.Description)
            .HasMaxLength(TimeType.MaxDescriptionLength);

        builder.Property(timeType => timeType.CountsAsWorkTime)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(timeType => timeType.IsPaid)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(timeType => timeType.RequiresObject)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(timeType => timeType.IsAbsence)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(timeType => timeType.Color)
            .HasMaxLength(TimeType.ColorLength)
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci");

        builder.Property(timeType => timeType.SortOrder)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(timeType => timeType.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(timeType => timeType.CreatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(timeType => timeType.UpdatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.HasIndex(timeType => timeType.Name)
            .IsUnique()
            .HasDatabaseName("UX_TimeTypes_Name");

        builder.HasIndex(timeType => timeType.Code)
            .IsUnique()
            .HasDatabaseName("UX_TimeTypes_Code");

        builder.HasIndex(timeType => new
            {
                timeType.IsActive,
                timeType.SortOrder,
                timeType.Name
            })
            .HasDatabaseName("IX_TimeTypes_Status_SortOrder_Name");
    }
}
