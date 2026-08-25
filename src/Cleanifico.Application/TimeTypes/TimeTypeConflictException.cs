namespace Cleanifico.Application.TimeTypes;

public sealed class TimeTypeConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
