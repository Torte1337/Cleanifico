using Cleanifico.Domain.Employees;
using Cleanifico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Tests;

public sealed class EmployeePersistenceTests
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
    public void Model_MapsRequiredFieldsDatesHoursAndAuditColumns()
    {
        using CleanificoDbContext context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Employee));
        Assert.NotNull(entityType);
        Assert.Equal("Employees", entityType.GetTableName());
        Assert.False(entityType.FindProperty(nameof(Employee.EmployeeNumber))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(Employee.FirstName))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(Employee.LastName))?.IsNullable);
        Assert.Equal("date", entityType.FindProperty(nameof(Employee.DateOfBirth))?.GetColumnType());
        Assert.Equal(7, entityType.FindProperty(nameof(Employee.WeeklyHours))?.GetPrecision());
        Assert.Equal(2, entityType.FindProperty(nameof(Employee.WeeklyHours))?.GetScale());
        Assert.Equal("datetime(6)", entityType.FindProperty(nameof(Employee.CreatedAtUtc))?.GetColumnType());
    }

    [Fact]
    public void Model_DeclaresUniqueNumberAndLookupIndexes()
    {
        using CleanificoDbContext context = CreateContext();
        var indexes = context.Model.FindEntityType(typeof(Employee))!.GetIndexes().ToArray();
        Assert.Contains(indexes, index => index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual([nameof(Employee.EmployeeNumber)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(Employee.IsActive), nameof(Employee.LastName), nameof(Employee.FirstName)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(Employee.City)]));
    }
}
