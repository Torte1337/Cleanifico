namespace Cleanifico.Contracts.TimeTypes;

public sealed record TimeTypeResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool CountsAsWorkTime,
    bool IsPaid,
    bool RequiresObject,
    bool IsAbsence,
    string? Color,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
