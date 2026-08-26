using Cleanifico.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .ValueGeneratedNever();

        builder.Property(customer => customer.CustomerNumber)
            .HasMaxLength(Customer.MaxCustomerNumberLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(customer => customer.CompanyName)
            .HasMaxLength(Customer.MaxCompanyNameLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(customer => customer.ContactFirstName)
            .HasMaxLength(Customer.MaxContactNameLength);
        builder.Property(customer => customer.ContactLastName)
            .HasMaxLength(Customer.MaxContactNameLength);
        builder.Property(customer => customer.Email)
            .HasMaxLength(Customer.MaxEmailLength);
        builder.Property(customer => customer.Phone)
            .HasMaxLength(Customer.MaxPhoneLength);
        builder.Property(customer => customer.Street)
            .HasMaxLength(Customer.MaxStreetLength);
        builder.Property(customer => customer.PostalCode)
            .HasMaxLength(Customer.MaxPostalCodeLength);
        builder.Property(customer => customer.City)
            .HasMaxLength(Customer.MaxCityLength);
        builder.Property(customer => customer.Country)
            .HasMaxLength(Customer.MaxCountryLength);
        builder.Property(customer => customer.Notes)
            .HasMaxLength(Customer.MaxNotesLength);
        builder.Property(customer => customer.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
        builder.Property(customer => customer.CreatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();
        builder.Property(customer => customer.UpdatedAtUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.HasIndex(customer => customer.CustomerNumber)
            .IsUnique()
            .HasDatabaseName("UX_Customers_CustomerNumber");
        builder.HasIndex(customer => new { customer.IsActive, customer.CompanyName })
            .HasDatabaseName("IX_Customers_Status_CompanyName");
        builder.HasIndex(customer => customer.City)
            .HasDatabaseName("IX_Customers_City");
    }
}
