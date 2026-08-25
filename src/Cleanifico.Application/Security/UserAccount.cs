namespace Cleanifico.Application.Security;

public sealed record UserAccount(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
