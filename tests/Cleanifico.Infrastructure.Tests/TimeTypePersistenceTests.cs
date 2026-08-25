using Cleanifico.Domain.TimeTypes;
using Cleanifico.Infrastructure.Persistence;
using Cleanifico.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Tests;

public sealed class TimeTypePersistenceTests
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
    public void Model_MapsTimeTypeFieldsAndDefaults()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(TimeType));

        Assert.NotNull(entityType);
        Assert.Equal("TimeTypes", entityType.GetTableName());
        Assert.Equal(TimeType.MaxNameLength, entityType.FindProperty(nameof(TimeType.Name))?.GetMaxLength());
        Assert.Equal(TimeType.MaxCodeLength, entityType.FindProperty(nameof(TimeType.Code))?.GetMaxLength());
        Assert.Equal(TimeType.ColorLength, entityType.FindProperty(nameof(TimeType.Color))?.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(TimeType.Name))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(TimeType.Code))?.IsNullable);
        Assert.Equal(true, entityType.FindProperty(nameof(TimeType.IsActive))?.GetDefaultValue());
        Assert.Equal(false, entityType.FindProperty(nameof(TimeType.CountsAsWorkTime))?.GetDefaultValue());
    }

    [Fact]
    public void Model_DeclaresUniqueAndLookupIndexes()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(TimeType));
        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToArray();

        Assert.Contains(indexes, index => index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual([nameof(TimeType.Name)]));
        Assert.Contains(indexes, index => index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual([nameof(TimeType.Code)]));
        Assert.Contains(indexes, index => index.Properties.Select(property => property.Name).SequenceEqual(
            [nameof(TimeType.IsActive), nameof(TimeType.SortOrder), nameof(TimeType.Name)]));
    }

    [Fact]
    public void Model_MapsTechnicalInitializationMarker()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(DataInitializationMarker));

        Assert.NotNull(entityType);
        Assert.Equal("DataInitializationMarkers", entityType.GetTableName());
        Assert.Equal([nameof(DataInitializationMarker.Key)], entityType.FindPrimaryKey()?.Properties.Select(p => p.Name));
    }
}
