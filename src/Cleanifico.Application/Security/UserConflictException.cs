namespace Cleanifico.Application.Security;

public sealed class UserConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
