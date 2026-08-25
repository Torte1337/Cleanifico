using System.ComponentModel.DataAnnotations;
using Cleanifico.Application.Security;
using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class IdentityUserAdministrationService(
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider,
    OwnerProtectionGate ownerProtectionGate,
    ILogger<IdentityUserAdministrationService> logger) : IUserAdministrationService
{
    public async Task<IReadOnlyList<UserAccount>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var results = new List<UserAccount>(users.Count);
        foreach (var user in users)
        {
            results.Add(await ToAccountAsync(user));
        }

        return results;
    }

    public async Task<UserAccount> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new UserNotFoundException(id);

        return await ToAccountAsync(user);
    }

    public async Task<UserAccount> CreateAsync(
        CreateUserInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var firstName = NormalizeName(input.FirstName, "firstName", "Der Vorname ist erforderlich.");
        var lastName = NormalizeName(input.LastName, "lastName", "Der Nachname ist erforderlich.");
        var email = NormalizeEmail(input.Email);
        var roles = NormalizeRoles(input.Roles);

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new UserConflictException("email", "Diese E-Mail-Adresse wird bereits verwendet.");
        }

        var user = ApplicationUser.Create(
            Guid.NewGuid(),
            firstName,
            lastName,
            email,
            input.IsActive,
            UtcNow());

        var createResult = await userManager.CreateAsync(user, input.Password);
        if (!createResult.Succeeded)
        {
            ThrowIdentityErrors(createResult);
        }

        var roleResult = await userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            ThrowIdentityErrors(roleResult, "roles");
        }

        logger.LogInformation("User {UserId} was created.", user.Id);
        return await ToAccountAsync(user);
    }

    public async Task<UserAccount> UpdateAsync(
        Guid id,
        UpdateUserInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindByIdAsync(id);
        var firstName = NormalizeName(input.FirstName, "firstName", "Der Vorname ist erforderlich.");
        var lastName = NormalizeName(input.LastName, "lastName", "Der Nachname ist erforderlich.");
        var email = NormalizeEmail(input.Email);

        var emailOwner = await userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != id)
        {
            throw new UserConflictException("email", "Diese E-Mail-Adresse wird bereits verwendet.");
        }

        await ownerProtectionGate.Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (user.IsActive && !input.IsActive)
            {
                await EnsureMayLoseActiveOwnerAsync(user);
            }

            user.Email = email;
            user.UserName = email;
            user.NormalizedEmail = userManager.NormalizeEmail(email);
            user.NormalizedUserName = userManager.NormalizeName(email);
            user.UpdateProfile(firstName, lastName, input.IsActive, UtcNow());

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ThrowIdentityErrors(result);
            }

            if (!input.IsActive)
            {
                await userManager.UpdateSecurityStampAsync(user);
                logger.LogInformation("User {UserId} was deactivated.", user.Id);
            }
        }
        finally
        {
            ownerProtectionGate.Semaphore.Release();
        }

        return await ToAccountAsync(user);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(id);
        if (user.IsActive)
        {
            return;
        }

        user.UpdateProfile(user.FirstName, user.LastName, true, UtcNow());
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ThrowIdentityErrors(result);
        }

        logger.LogInformation("User {UserId} was activated.", user.Id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(id);
        if (!user.IsActive)
        {
            return;
        }

        await ownerProtectionGate.Semaphore.WaitAsync(cancellationToken);
        try
        {
            await EnsureMayLoseActiveOwnerAsync(user);
            user.UpdateProfile(user.FirstName, user.LastName, false, UtcNow());

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ThrowIdentityErrors(result);
            }

            await userManager.UpdateSecurityStampAsync(user);
            logger.LogInformation("User {UserId} was deactivated.", user.Id);
        }
        finally
        {
            ownerProtectionGate.Semaphore.Release();
        }
    }

    public Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SecurityRoles.All);
    }

    public async Task<UserAccount> UpdateRolesAsync(
        Guid id,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoles = NormalizeRoles(roles);
        var user = await FindByIdAsync(id);

        await ownerProtectionGate.Semaphore.WaitAsync(cancellationToken);
        try
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToAdd = normalizedRoles.Except(currentRoles, StringComparer.Ordinal).ToArray();
            var rolesToRemove = currentRoles.Except(normalizedRoles, StringComparer.Ordinal).ToArray();

            if (user.IsActive && rolesToRemove.Contains(SecurityRoles.Owner, StringComparer.Ordinal))
            {
                await EnsureMayLoseActiveOwnerAsync(user);
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    ThrowIdentityErrors(addResult, "roles");
                }
            }

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    ThrowIdentityErrors(removeResult, "roles");
                }
            }

            user.UpdateProfile(user.FirstName, user.LastName, user.IsActive, UtcNow());
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors(updateResult);
            }

            await userManager.UpdateSecurityStampAsync(user);
            logger.LogInformation("Roles for user {UserId} were changed.", user.Id);
        }
        finally
        {
            ownerProtectionGate.Semaphore.Release();
        }

        return await ToAccountAsync(user);
    }

    private async Task<ApplicationUser> FindByIdAsync(Guid id) =>
        await userManager.FindByIdAsync(id.ToString()) ?? throw new UserNotFoundException(id);

    private async Task EnsureMayLoseActiveOwnerAsync(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, SecurityRoles.Owner))
        {
            return;
        }

        var owners = await userManager.GetUsersInRoleAsync(SecurityRoles.Owner);
        if (!owners.Any(owner => owner.Id != user.Id && owner.IsActive))
        {
            throw new UserConflictException(
                "owner",
                "Der letzte aktive Inhaber darf nicht deaktiviert oder seiner Inhaberrolle beraubt werden.");
        }
    }

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserAccount(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.IsActive,
            [.. roles.OrderBy(RoleSortOrder)],
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }

    private static int RoleSortOrder(string role)
    {
        for (var index = 0; index < SecurityRoles.All.Count; index++)
        {
            if (string.Equals(SecurityRoles.All[index], role, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    private static string NormalizeName(string? value, string field, string missingMessage)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new UserValidationException(field, missingMessage);
        }

        if (normalized.Length > ApplicationUser.MaxNameLength)
        {
            throw new UserValidationException(
                field,
                $"Der Wert darf höchstens {ApplicationUser.MaxNameLength} Zeichen lang sein.");
        }

        return normalized;
    }

    private static string NormalizeEmail(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new UserValidationException("email", "Die E-Mail-Adresse ist erforderlich.");
        }

        if (normalized.Length > 256 || !new EmailAddressAttribute().IsValid(normalized))
        {
            throw new UserValidationException("email", "Bitte geben Sie eine gültige E-Mail-Adresse ein.");
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeRoles(IReadOnlyCollection<string>? roles)
    {
        if (roles is null || roles.Count == 0)
        {
            throw new UserValidationException("roles", "Mindestens eine Rolle ist erforderlich.");
        }

        var normalized = new List<string>();
        foreach (var role in roles)
        {
            var knownRole = SecurityRoles.All.FirstOrDefault(
                candidate => string.Equals(candidate, role?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (knownRole is null)
            {
                throw new UserValidationException("roles", $"Die Rolle '{role}' ist unbekannt.");
            }

            if (!normalized.Contains(knownRole, StringComparer.Ordinal))
            {
                normalized.Add(knownRole);
            }
        }

        return normalized;
    }

    private static void ThrowIdentityErrors(IdentityResult result, string? field = null)
    {
        var errors = result.Errors.ToArray();
        if (errors.Any(error =>
                error.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                    or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            throw new UserConflictException("email", "Diese E-Mail-Adresse wird bereits verwendet.");
        }

        var errorField = field ?? (errors.Any(error => error.Code.StartsWith("Password", StringComparison.Ordinal))
            ? "password"
            : "user");

        throw new UserValidationException(
            errorField,
            string.Join(" ", errors.Select(error => error.Description)));
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
