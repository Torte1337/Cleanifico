using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Application.TimeTypes;

public sealed class TimeTypeInitializer(
    ITimeTypeRepository repository,
    TimeProvider timeProvider) : ITimeTypeInitializer
{
    private static readonly TimeTypeInput[] Defaults =
    [
        new("Arbeitszeit", "ARB", null, true, true, true, false, "#2F855A", 10),
        new("Pause", "PAU", null, false, false, false, false, "#D69E2E", 20),
        new("Fahrzeit", "FAH", null, true, true, true, false, "#3182CE", 30),
        new("Urlaub", "URL", null, true, true, false, true, "#805AD5", 40),
        new("Krankheit", "KRK", null, true, true, false, true, "#E53E3E", 50),
        new("Schulung", "SCH", null, true, true, false, false, "#319795", 60),
        new("Besprechung", "BES", null, true, true, false, false, "#718096", 70)
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var initializedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var defaults = Defaults.Select(input => TimeType.Create(
            Guid.NewGuid(),
            input.Name!,
            input.Code!,
            input.Description,
            input.CountsAsWorkTime,
            input.IsPaid,
            input.RequiresObject,
            input.IsAbsence,
            input.Color,
            input.SortOrder,
            initializedAtUtc)).ToArray();

        await repository.InitializeDefaultsAsync(defaults, initializedAtUtc, cancellationToken);
    }
}
