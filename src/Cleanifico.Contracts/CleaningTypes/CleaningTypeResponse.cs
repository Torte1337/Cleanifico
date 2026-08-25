namespace Cleanifico.Contracts.CleaningTypes;

public sealed record CleaningTypeResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
