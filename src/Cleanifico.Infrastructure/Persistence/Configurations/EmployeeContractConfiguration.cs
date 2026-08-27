using Cleanifico.Domain.EmployeeContracts;
using Cleanifico.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cleanifico.Infrastructure.Persistence.Configurations;

public sealed class EmployeeContractConfiguration : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("EmployeeContracts");
        builder.HasCharSet("utf8mb4");

        builder.HasKey(contract => contract.Id);
        builder.Property(contract => contract.Id)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .ValueGeneratedNever();
        builder.Property(contract => contract.EmployeeId)
            .HasColumnType("char(36)")
            .HasCharSet("ascii")
            .UseCollation("ascii_general_ci")
            .IsFixedLength()
            .IsRequired();
        builder.Property(contract => contract.ContractNumber)
            .HasMaxLength(EmployeeContract.MaxContractNumberLength)
            .UseCollation("utf8mb4_0900_ai_ci")
            .IsRequired();
        builder.Property(contract => contract.StartDate).HasColumnType("date").IsRequired();
        builder.Property(contract => contract.EndDate).HasColumnType("date");
        builder.Property(contract => contract.IsPermanent).IsRequired();
        builder.Property(contract => contract.EmploymentType)
            .HasMaxLength(EmployeeContract.MaxEmploymentTypeLength);
        builder.Property(contract => contract.WeeklyHours).HasPrecision(7, 2).IsRequired();
        builder.Property(contract => contract.MonthlyTargetHours).HasPrecision(7, 2).IsRequired();
        builder.Property(contract => contract.VacationDaysPerYear).HasPrecision(5, 2).IsRequired();
        builder.Property(contract => contract.ProbationEndDate).HasColumnType("date");
        builder.Property(contract => contract.Notes).HasMaxLength(EmployeeContract.MaxNotesLength);
        builder.Property(contract => contract.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(contract => contract.CreatedAtUtc).HasColumnType("datetime(6)").IsRequired();
        builder.Property(contract => contract.UpdatedAtUtc).HasColumnType("datetime(6)").IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(contract => contract.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasIndex(contract => contract.ContractNumber)
            .IsUnique()
            .HasDatabaseName("UX_EmployeeContracts_ContractNumber");
        builder.HasIndex(contract => new
            {
                contract.EmployeeId,
                contract.IsActive,
                contract.StartDate,
                contract.EndDate
            })
            .HasDatabaseName("IX_EmployeeContracts_Employee_Status_Period");
        builder.HasIndex(contract => new { contract.IsActive, contract.StartDate })
            .HasDatabaseName("IX_EmployeeContracts_Status_StartDate");
    }
}
