namespace Cleanifico.Application.CleaningTypes;

public sealed record CleaningTypeInput(
    string? Name,
    string? Code,
    string? Description,
    int SortOrder);
