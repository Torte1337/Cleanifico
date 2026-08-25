namespace Cleanifico.Application.TimeTypes;

public sealed record TimeTypeInput(
    string? Name,
    string? Code,
    string? Description,
    bool CountsAsWorkTime,
    bool IsPaid,
    bool RequiresObject,
    bool IsAbsence,
    string? Color,
    int SortOrder);
