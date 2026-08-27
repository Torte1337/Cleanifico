using Cleanifico.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.Id)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .ValueGeneratedNever();
        builder.Property(employee => employee.EmployeeNumber)
            .HasMaxLength(Employee.MaxEmployeeNumberLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(employee => employee.FirstName)
            .HasMaxLength(Employee.MaxNameLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(employee => employee.LastName)
            .HasMaxLength(Employee.MaxNameLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(employee => employee.Street).HasMaxLength(Employee.MaxStreetLength);
        builder.Property(employee => employee.PostalCode).HasMaxLength(Employee.MaxPostalCodeLength);
        builder.Property(employee => employee.City).HasMaxLength(Employee.MaxCityLength);
        builder.Property(employee => employee.Country).HasMaxLength(Employee.MaxCountryLength);
        builder.Property(employee => employee.Email).HasMaxLength(Employee.MaxEmailLength);
        builder.Property(employee => employee.Phone).HasMaxLength(Employee.MaxPhoneLength);
        builder.Property(employee => employee.MobilePhone).HasMaxLength(Employee.MaxPhoneLength);
        builder.Property(employee => employee.DateOfBirth).HasColumnType("date");
        builder.Property(employee => employee.Notes).HasMaxLength(Employee.MaxNotesLength);
        builder.Property(employee => employee.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(employee => employee.CreatedAtUtc).HasColumnType("datetime(6)").IsRequired();
        builder.Property(employee => employee.UpdatedAtUtc).HasColumnType("datetime(6)").IsRequired();

        builder.HasIndex(employee => employee.EmployeeNumber)
            .IsUnique()
            .HasDatabaseName("UX_Employees_EmployeeNumber");
        builder.HasIndex(employee => new { employee.IsActive, employee.LastName, employee.FirstName })
            .HasDatabaseName("IX_Employees_Status_Name");
        builder.HasIndex(employee => employee.City)
            .HasDatabaseName("IX_Employees_City");
    }
}
