using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.Customers;

public sealed class CreateCustomerRequest
{
    [Required(ErrorMessage = "Die Kundennummer ist erforderlich.")]
    [StringLength(50, ErrorMessage = "Die Kundennummer darf höchstens 50 Zeichen lang sein.")]
    public string CustomerNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Der Firmenname ist erforderlich.")]
    [StringLength(200, ErrorMessage = "Der Firmenname darf höchstens 200 Zeichen lang sein.")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Der Vorname darf höchstens 100 Zeichen lang sein.")]
    public string? ContactFirstName { get; set; }

    [StringLength(100, ErrorMessage = "Der Nachname darf höchstens 100 Zeichen lang sein.")]
    public string? ContactLastName { get; set; }

    [EmailAddress(ErrorMessage = "Die E-Mail-Adresse ist ungültig.")]
    [StringLength(320, ErrorMessage = "Die E-Mail-Adresse darf höchstens 320 Zeichen lang sein.")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "Die Telefonnummer darf höchstens 50 Zeichen lang sein.")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Die Straße darf höchstens 200 Zeichen lang sein.")]
    public string? Street { get; set; }

    [StringLength(20, ErrorMessage = "Die PLZ darf höchstens 20 Zeichen lang sein.")]
    public string? PostalCode { get; set; }

    [StringLength(100, ErrorMessage = "Der Ort darf höchstens 100 Zeichen lang sein.")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "Das Land darf höchstens 100 Zeichen lang sein.")]
    public string? Country { get; set; }

    [StringLength(2_000, ErrorMessage = "Die Notizen dürfen höchstens 2000 Zeichen lang sein.")]
    public string? Notes { get; set; }
}
