using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class CleaningObjectConfiguration : IEntityTypeConfiguration<CleaningObject>
{
    public void Configure(EntityTypeBuilder<CleaningObject> builder)
    {
        builder.ToTable("CleaningObjects");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnType("char(36)").HasCharSet("ascii")
            .UseCollation("ascii_general_ci").IsFixedLength().ValueGeneratedNever();
        builder.Property(item => item.CustomerId).HasColumnType("char(36)").HasCharSet("ascii")
            .UseCollation("ascii_general_ci").IsFixedLength().IsRequired();
        builder.Property(item => item.ObjectNumber).HasMaxLength(CleaningObject.MaxObjectNumberLength)
            .UseCollation("utf8mb4_0900_ai_ci").IsRequired();
        builder.Property(item => item.Name).HasMaxLength(CleaningObject.MaxNameLength).IsRequired();
        builder.Property(item => item.Street).HasMaxLength(CleaningObject.MaxStreetLength);
        builder.Property(item => item.PostalCode).HasMaxLength(CleaningObject.MaxPostalCodeLength);
        builder.Property(item => item.City).HasMaxLength(CleaningObject.MaxCityLength);
        builder.Property(item => item.Country).HasMaxLength(CleaningObject.MaxCountryLength);
        builder.Property(item => item.ContactFirstName).HasMaxLength(CleaningObject.MaxContactNameLength);
        builder.Property(item => item.ContactLastName).HasMaxLength(CleaningObject.MaxContactNameLength);
        builder.Property(item => item.ContactEmail).HasMaxLength(CleaningObject.MaxEmailLength);
        builder.Property(item => item.ContactPhone).HasMaxLength(CleaningObject.MaxPhoneLength);
        builder.Property(item => item.AccessNotes).HasMaxLength(CleaningObject.MaxNotesLength);
        builder.Property(item => item.CleaningNotes).HasMaxLength(CleaningObject.MaxNotesLength);
        builder.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(item => item.CreatedAtUtc).HasColumnType("datetime(6)").IsRequired();
        builder.Property(item => item.UpdatedAtUtc).HasColumnType("datetime(6)").IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(item => item.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasIndex(item => item.ObjectNumber).IsUnique()
            .HasDatabaseName("UX_CleaningObjects_ObjectNumber");
        builder.HasIndex(item => new { item.CustomerId, item.IsActive, item.Name })
            .HasDatabaseName("IX_CleaningObjects_Customer_Status_Name");
        builder.HasIndex(item => new { item.IsActive, item.Name })
            .HasDatabaseName("IX_CleaningObjects_Status_Name");
        builder.HasIndex(item => item.City).HasDatabaseName("IX_CleaningObjects_City");
    }
}
