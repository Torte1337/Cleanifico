using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Customers;
using Cleanifico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Cleanifico.Infrastructure.Tests;

public sealed class CleaningObjectPersistenceTests
{
    private static CleanificoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CleanificoDbContext>()
            .UseMySql("Server=localhost;Database=cleanifico_model_test;User=model_test;Password=not-used", new MySqlServerVersion(new Version(8, 4, 0))).Options;
        return new CleanificoDbContext(options);
    }

    [Fact]
    public void Model_MapsFieldsUniqueNumberAndLookupIndexes()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(CleaningObject));
        Assert.NotNull(entity); Assert.Equal("CleaningObjects", entity.GetTableName());
        Assert.False(entity.FindProperty(nameof(CleaningObject.ObjectNumber))?.IsNullable);
        Assert.False(entity.FindProperty(nameof(CleaningObject.CustomerId))?.IsNullable);
        Assert.Equal(CleaningObject.MaxEmailLength, entity.FindProperty(nameof(CleaningObject.ContactEmail))?.GetMaxLength());
        var indexes = entity.GetIndexes().ToArray();
        Assert.Contains(indexes, i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual([nameof(CleaningObject.ObjectNumber)]));
        Assert.Contains(indexes, i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(CleaningObject.CustomerId), nameof(CleaningObject.IsActive), nameof(CleaningObject.Name)]));
    }

    [Fact]
    public void Model_UsesRequiredRestrictCustomerForeignKey()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(CleaningObject));
        var foreignKey = Assert.Single(entity!.GetForeignKeys());
        Assert.Equal(typeof(Customer), foreignKey.PrincipalEntityType.ClrType);
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }
}
