using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.Tests;

public sealed class CleaningTypeTests
{
    private static readonly DateTime CreatedAt = new(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesValuesAndInitializesLifecycle()
    {
        var id = Guid.NewGuid();

        var cleaningType = CleaningType.Create(
            id,
            "  Unterhaltsreinigung  ",
            "  ur  ",
            "  Regelmäßige Reinigung  ",
            10,
            CreatedAt);

        Assert.Equal(id, cleaningType.Id);
        Assert.Equal("Unterhaltsreinigung", cleaningType.Name);
        Assert.Equal("UR", cleaningType.Code);
        Assert.Equal("Regelmäßige Reinigung", cleaningType.Description);
        Assert.True(cleaningType.IsActive);
        Assert.Equal(10, cleaningType.SortOrder);
        Assert.Equal(CreatedAt, cleaningType.CreatedAtUtc);
        Assert.Equal(CreatedAt, cleaningType.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingName(string? name)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Create(name: name));

        Assert.Contains("name", exception.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingCode(string? code)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Create(code: code));

        Assert.Contains("code", exception.Errors.Keys);
    }

    [Fact]
    public void Create_RejectsNegativeSortOrder()
    {
        var exception = Assert.Throws<DomainValidationException>(() => Create(sortOrder: -1));

        Assert.Contains("sortOrder", exception.Errors.Keys);
    }

    [Fact]
    public void Create_RejectsNonUtcTimestamp()
    {
        var localTime = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Local);

        var exception = Assert.Throws<DomainValidationException>(() => Create(createdAt: localTime));

        Assert.Contains("createdAtUtc", exception.Errors.Keys);
    }

    [Fact]
    public void Update_ReplacesMutableValuesAndTimestamp()
    {
        var cleaningType = Create();
        var updatedAt = CreatedAt.AddHours(1);

        cleaningType.Update(" Grundreinigung ", " gr ", " ", 20, updatedAt);

        Assert.Equal("Grundreinigung", cleaningType.Name);
        Assert.Equal("GR", cleaningType.Code);
        Assert.Null(cleaningType.Description);
        Assert.Equal(20, cleaningType.SortOrder);
        Assert.Equal(updatedAt, cleaningType.UpdatedAtUtc);
        Assert.Equal(CreatedAt, cleaningType.CreatedAtUtc);
    }

    [Fact]
    public void InvalidUpdate_DoesNotPartiallyChangeEntity()
    {
        var cleaningType = Create();

        Assert.Throws<DomainValidationException>(() =>
            cleaningType.Update("Geänderter Name", " ", null, 20, CreatedAt.AddHours(1)));

        Assert.Equal("Unterhaltsreinigung", cleaningType.Name);
        Assert.Equal("UR", cleaningType.Code);
        Assert.Equal(10, cleaningType.SortOrder);
        Assert.Equal(CreatedAt, cleaningType.UpdatedAtUtc);
    }

    [Fact]
    public void DeactivateAndActivate_ChangeStatusAndTimestamp()
    {
        var cleaningType = Create();
        var deactivatedAt = CreatedAt.AddMinutes(30);
        var activatedAt = CreatedAt.AddHours(1);

        cleaningType.Deactivate(deactivatedAt);
        Assert.False(cleaningType.IsActive);
        Assert.Equal(deactivatedAt, cleaningType.UpdatedAtUtc);

        cleaningType.Activate(activatedAt);
        Assert.True(cleaningType.IsActive);
        Assert.Equal(activatedAt, cleaningType.UpdatedAtUtc);
    }

    [Fact]
    public void RepeatingCurrentStatus_DoesNotChangeTimestamp()
    {
        var cleaningType = Create();

        cleaningType.Activate(CreatedAt.AddHours(2));

        Assert.Equal(CreatedAt, cleaningType.UpdatedAtUtc);
    }

    private static CleaningType Create(
        string? name = "Unterhaltsreinigung",
        string? code = "UR",
        int sortOrder = 10,
        DateTime? createdAt = null) =>
        CleaningType.Create(
            Guid.NewGuid(),
            name!,
            code!,
            null,
            sortOrder,
            createdAt ?? CreatedAt);
}
