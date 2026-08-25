using Cleanifico.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class DataInitializationMarkerConfiguration
    : IEntityTypeConfiguration<DataInitializationMarker>
{
    public void Configure(EntityTypeBuilder<DataInitializationMarker> builder)
    {
        builder.ToTable("DataInitializationMarkers");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(marker => marker.Key);
        builder.Property(marker => marker.Key)
            .HasMaxLength(100)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(marker => marker.CompletedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();
    }
}
