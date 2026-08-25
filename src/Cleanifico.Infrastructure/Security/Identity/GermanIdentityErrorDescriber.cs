using Microsoft.AspNetCore.Identity;

namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class GermanIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = "Diese E-Mail-Adresse wird bereits verwendet."
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = "Diese E-Mail-Adresse wird bereits verwendet."
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = "Bitte geben Sie eine gültige E-Mail-Adresse ein."
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"Das Passwort muss mindestens {length} Zeichen lang sein."
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "Das Passwort muss mindestens eine Ziffer enthalten."
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "Das Passwort muss mindestens einen Kleinbuchstaben enthalten."
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "Das Passwort muss mindestens einen Großbuchstaben enthalten."
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "Das Passwort muss mindestens ein Sonderzeichen enthalten."
    };
}
