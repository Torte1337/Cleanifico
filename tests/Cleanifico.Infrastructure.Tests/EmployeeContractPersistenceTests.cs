using Cleanifico.Domain.EmployeeContracts;
using Cleanifico.Domain.Employees;
using Cleanifico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Tests;

public sealed class EmployeeContractPersistenceTests
{
    private static CleanificoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CleanificoDbContext>()
            .UseMySql(
                "Server=localhost;Database=cleanifico_model_test;User=model_test;Password=not-used",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        return new(options);
    }

    [Fact]
    public void Model_MapsFieldsUniqueNumberAndLookupIndexes()
    {
        using CleanificoDbContext context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(EmployeeContract));

        Assert.NotNull(entityType);
        Assert.Equal("EmployeeContracts", entityType.GetTableName());
        Assert.False(entityType.FindProperty(nameof(EmployeeContract.ContractNumber))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(EmployeeContract.EmployeeId))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(EmployeeContract.StartDate))?.IsNullable);
        Assert.Equal("date", entityType.FindProperty(nameof(EmployeeContract.StartDate))?.GetColumnType());
        Assert.Equal(7, entityType.FindProperty(nameof(EmployeeContract.WeeklyHours))?.GetPrecision());
        Assert.Equal(2, entityType.FindProperty(nameof(EmployeeContract.WeeklyHours))?.GetScale());
        Assert.Equal(5, entityType.FindProperty(nameof(EmployeeContract.VacationDaysPerYear))?.GetPrecision());
        Assert.Contains(entityType.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(EmployeeContract.ContractNumber)]));
        Assert.Contains(entityType.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual([
                nameof(EmployeeContract.EmployeeId),
                nameof(EmployeeContract.IsActive),
                nameof(EmployeeContract.StartDate),
                nameof(EmployeeContract.EndDate)]));
    }

    [Fact]
    public void Model_DeclaresRequiredRestrictEmployeeForeignKey()
    {
        using CleanificoDbContext context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(EmployeeContract))!;
        var foreignKey = Assert.Single(entityType.GetForeignKeys());

        Assert.Equal(typeof(Employee), foreignKey.PrincipalEntityType.ClrType);
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }
}
