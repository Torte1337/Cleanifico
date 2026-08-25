using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.Users;

public sealed class UpdateUserRequest
{
    [Required(ErrorMessage = "Der Vorname ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Der Vorname darf höchstens 100 Zeichen lang sein.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Der Nachname ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Der Nachname darf höchstens 100 Zeichen lang sein.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Die E-Mail-Adresse ist erforderlich.")]
    [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
    [StringLength(256, ErrorMessage = "Die E-Mail-Adresse darf höchstens 256 Zeichen lang sein.")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
