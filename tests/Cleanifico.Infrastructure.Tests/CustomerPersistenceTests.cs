using Cleanifico.Domain.Customers;
using Cleanifico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Tests;

public sealed class CustomerPersistenceTests
{
    private static CleanificoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CleanificoDbContext>()
            .UseMySql(
                "Server=localhost;Database=cleanifico_model_test;User=model_test;Password=not-used",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        return new CleanificoDbContext(options);
    }

    [Fact]
    public void Model_MapsCustomerFieldsAndAuditColumns()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        Assert.Equal("Customers", entityType.GetTableName());
        Assert.Equal(
            Customer.MaxCustomerNumberLength,
            entityType.FindProperty(nameof(Customer.CustomerNumber))?.GetMaxLength());
        Assert.Equal(
            Customer.MaxCompanyNameLength,
            entityType.FindProperty(nameof(Customer.CompanyName))?.GetMaxLength());
        Assert.Equal(
            Customer.MaxEmailLength,
            entityType.FindProperty(nameof(Customer.Email))?.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(Customer.CustomerNumber))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(Customer.CompanyName))?.IsNullable);
        Assert.Equal("datetime(6)", entityType.FindProperty(nameof(Customer.CreatedAtUtc))?.GetColumnType());
        Assert.Equal("datetime(6)", entityType.FindProperty(nameof(Customer.UpdatedAtUtc))?.GetColumnType());
    }

    [Fact]
    public void Model_DeclaresUniqueNumberAndLookupIndexes()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Customer));
        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToArray();

        Assert.Contains(indexes, index => index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Customer.CustomerNumber)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(Customer.IsActive), nameof(Customer.CompanyName)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(Customer.City)]));
    }
}
