namespace Cleanifico.Application.CleaningTypes;

public sealed class CleaningTypeConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
