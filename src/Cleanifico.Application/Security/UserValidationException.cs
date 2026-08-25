namespace Cleanifico.Application.Security;

public sealed class UserValidationException : Exception
{
    public UserValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }

    public UserValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Die Benutzerdaten sind ungültig.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
