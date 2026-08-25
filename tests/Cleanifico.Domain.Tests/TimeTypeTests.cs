using Cleanifico.Domain.Common;
using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Domain.Tests;

public sealed class TimeTypeTests
{
    private static readonly DateTime CreatedAt =
        new(2026, 8, 25, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesValuesAndInitializesAllProperties()
    {
        var timeType = Create(
            name: " Arbeitszeit ",
            code: " arb ",
            description: " Reguläre Arbeitszeit ",
            countsAsWorkTime: true,
            isPaid: true,
            requiresObject: true,
            color: " #2f855a ");

        Assert.Equal("Arbeitszeit", timeType.Name);
        Assert.Equal("ARB", timeType.Code);
        Assert.Equal("Reguläre Arbeitszeit", timeType.Description);
        Assert.True(timeType.CountsAsWorkTime);
        Assert.True(timeType.IsPaid);
        Assert.True(timeType.RequiresObject);
        Assert.False(timeType.IsAbsence);
        Assert.Equal("#2F855A", timeType.Color);
        Assert.True(timeType.IsActive);
        Assert.Equal(CreatedAt, timeType.CreatedAtUtc);
        Assert.Equal(CreatedAt, timeType.UpdatedAtUtc);
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
    public void Update_ChangesEveryConfigurableProperty()
    {
        var timeType = Create();
        var updatedAt = CreatedAt.AddHours(1);

        timeType.Update(
            " Urlaub ",
            " url ",
            " Erholungsurlaub ",
            true,
            true,
            false,
            true,
            "#805ad5",
            -10,
            updatedAt);

        Assert.Equal("Urlaub", timeType.Name);
        Assert.Equal("URL", timeType.Code);
        Assert.Equal("Erholungsurlaub", timeType.Description);
        Assert.True(timeType.CountsAsWorkTime);
        Assert.True(timeType.IsPaid);
        Assert.False(timeType.RequiresObject);
        Assert.True(timeType.IsAbsence);
        Assert.Equal("#805AD5", timeType.Color);
        Assert.Equal(-10, timeType.SortOrder);
        Assert.Equal(updatedAt, timeType.UpdatedAtUtc);
    }

    [Fact]
    public void ActivateAndDeactivate_ChangeLifecycle()
    {
        var timeType = Create();

        timeType.Deactivate(CreatedAt.AddMinutes(1));
        Assert.False(timeType.IsActive);

        timeType.Activate(CreatedAt.AddMinutes(2));
        Assert.True(timeType.IsActive);
        Assert.Equal(CreatedAt.AddMinutes(2), timeType.UpdatedAtUtc);
    }

    private static TimeType Create(
        string? name = "Arbeitszeit",
        string? code = "ARB",
        string? description = null,
        bool countsAsWorkTime = false,
        bool isPaid = false,
        bool requiresObject = false,
        bool isAbsence = false,
        string? color = null) =>
        TimeType.Create(
            Guid.NewGuid(),
            name!,
            code!,
            description,
            countsAsWorkTime,
            isPaid,
            requiresObject,
            isAbsence,
            color,
            10,
            CreatedAt);
}
