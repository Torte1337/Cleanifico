namespace Cleanifico.Application.Security;

public sealed class UserNotFoundException(Guid id)
    : Exception($"Der Benutzer mit der ID '{id}' wurde nicht gefunden.");
