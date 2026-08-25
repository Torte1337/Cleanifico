using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Infrastructure.Persistence;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cleanifico.Infrastructure.Tests;

public sealed class CleaningTypePersistenceTests
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
    public void Model_MapsCleaningTypeToExpectedTableAndColumns()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CleaningType));

        Assert.NotNull(entityType);
        Assert.Equal("CleaningTypes", entityType.GetTableName());
        Assert.Equal(CleaningType.MaxNameLength, entityType.FindProperty(nameof(CleaningType.Name))?.GetMaxLength());
        Assert.Equal(CleaningType.MaxCodeLength, entityType.FindProperty(nameof(CleaningType.Code))?.GetMaxLength());
        Assert.Equal(
            CleaningType.MaxDescriptionLength,
            entityType.FindProperty(nameof(CleaningType.Description))?.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(CleaningType.Name))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(CleaningType.Code))?.IsNullable);
    }

    [Fact]
    public void Model_DeclaresUniqueNameAndCodeIndexes()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CleaningType));
        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToArray();

        Assert.Contains(indexes, index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([nameof(CleaningType.Name)]));
        Assert.Contains(indexes, index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([nameof(CleaningType.Code)]));
    }

    [Fact]
    public void Model_DeclaresStatusSortAndNameLookupIndex()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CleaningType));
        Assert.NotNull(entityType);

        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(CleaningType.IsActive), nameof(CleaningType.SortOrder), nameof(CleaningType.Name)]));
    }

    [Fact]
    public void Model_MapsApplicationUserFieldsAndUniqueEmailIndex()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ApplicationUser));

        Assert.NotNull(entityType);
        Assert.Equal("AspNetUsers", entityType.GetTableName());
        Assert.False(entityType.FindProperty(nameof(ApplicationUser.FirstName))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(ApplicationUser.LastName))?.IsNullable);
        Assert.False(entityType.FindProperty(nameof(ApplicationUser.IsActive))?.IsNullable);
        Assert.Contains(entityType.GetIndexes(), index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual([nameof(ApplicationUser.NormalizedEmail)]));
    }
}
