namespace Cleanifico.Application.Security;

public sealed record CreateUserInput(
    string? FirstName,
    string? LastName,
    string? Email,
    string Password,
    IReadOnlyCollection<string> Roles,
    bool IsActive);

public sealed record UpdateUserInput(
    string? FirstName,
    string? LastName,
    string? Email,
    bool IsActive);
