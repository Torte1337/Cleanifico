using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.Authentication;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Die E-Mail-Adresse ist erforderlich.")]
    [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Das Passwort ist erforderlich.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
