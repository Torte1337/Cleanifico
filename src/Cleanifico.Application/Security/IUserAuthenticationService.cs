namespace Cleanifico.Application.Security;

public interface IUserAuthenticationService
{
    Task<SignInOutcome> PasswordSignInAsync(
        string? email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}

public enum SignInOutcome
{
    Success,
    InvalidCredentials,
    LockedOut,
    Inactive
}
