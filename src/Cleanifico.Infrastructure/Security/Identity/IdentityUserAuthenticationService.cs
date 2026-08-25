using Cleanifico.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class IdentityUserAuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<IdentityUserAuthenticationService> logger) : IUserAuthenticationService
{
    public async Task<SignInOutcome> PasswordSignInAsync(
        string? email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = email?.Trim();
        var user = string.IsNullOrWhiteSpace(normalizedEmail)
            ? null
            : await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            logger.LogWarning("Login failed for an unknown account.");
            return SignInOutcome.InvalidCredentials;
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login failed for inactive user {UserId}.", user.Id);
            return SignInOutcome.Inactive;
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Login succeeded for user {UserId}.", user.Id);
            return SignInOutcome.Success;
        }

        logger.LogWarning("Login failed for user {UserId}.", user.Id);
        return result.IsLockedOut ? SignInOutcome.LockedOut : SignInOutcome.InvalidCredentials;
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
    }
}
