namespace Cleanifico.Application.CleaningObjects;

public sealed class CleaningObjectConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
