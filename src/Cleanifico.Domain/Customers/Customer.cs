using System.ComponentModel.DataAnnotations;
using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.Customers;

public sealed class Customer
{
    public const int MaxCustomerNumberLength = 50;
    public const int MaxCompanyNameLength = 200;
    public const int MaxContactNameLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 50;
    public const int MaxStreetLength = 200;
    public const int MaxPostalCodeLength = 20;
    public const int MaxCityLength = 100;
    public const int MaxCountryLength = 100;
    public const int MaxNotesLength = 2_000;

    private Customer()
    {
        CustomerNumber = string.Empty;
        CompanyName = string.Empty;
    }

    private Customer(
        Guid id,
        CustomerData data,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("id", "Die ID darf nicht leer sein.");
        }

        var normalized = Normalize(data);
        Id = id;
        Apply(normalized);
        IsActive = true;
        CreatedAtUtc = EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string CustomerNumber { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string? ContactFirstName { get; private set; }
    public string? ContactLastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Street { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Customer Create(Guid id, CustomerData data, DateTime createdAtUtc) =>
        new(id, data, createdAtUtc);

    public void Update(CustomerData data, DateTime updatedAtUtc)
    {
        var normalized = Normalize(data);
        var validatedUpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        Apply(normalized);
        UpdatedAtUtc = validatedUpdatedAtUtc;
    }

    public void Activate(DateTime updatedAtUtc)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public static string NormalizeCustomerNumber(string? value) =>
        NormalizeRequired(value, "customerNumber", "Die Kundennummer ist erforderlich.", MaxCustomerNumberLength);

    public static string NormalizeCompanyName(string? value) =>
        NormalizeRequired(value, "companyName", "Der Firmenname ist erforderlich.", MaxCompanyNameLength);

    private static CustomerData Normalize(CustomerData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var email = NormalizeOptional(data.Email, "email", MaxEmailLength);
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
        {
            throw new DomainValidationException("email", "Die E-Mail-Adresse ist ungültig.");
        }

        return data with
        {
            CustomerNumber = NormalizeCustomerNumber(data.CustomerNumber),
            CompanyName = NormalizeCompanyName(data.CompanyName),
            ContactFirstName = NormalizeOptional(
                data.ContactFirstName,
                "contactFirstName",
                MaxContactNameLength),
            ContactLastName = NormalizeOptional(
                data.ContactLastName,
                "contactLastName",
                MaxContactNameLength),
            Email = email,
            Phone = NormalizeOptional(data.Phone, "phone", MaxPhoneLength),
            Street = NormalizeOptional(data.Street, "street", MaxStreetLength),
            PostalCode = NormalizeOptional(data.PostalCode, "postalCode", MaxPostalCodeLength),
            City = NormalizeOptional(data.City, "city", MaxCityLength),
            Country = NormalizeOptional(data.Country, "country", MaxCountryLength),
            Notes = NormalizeOptional(data.Notes, "notes", MaxNotesLength)
        };
    }

    private void Apply(CustomerData data)
    {
        CustomerNumber = data.CustomerNumber!;
        CompanyName = data.CompanyName!;
        ContactFirstName = data.ContactFirstName;
        ContactLastName = data.ContactLastName;
        Email = data.Email;
        Phone = data.Phone;
        Street = data.Street;
        PostalCode = data.PostalCode;
        City = data.City;
        Country = data.Country;
        Notes = data.Notes;
    }

    private static string NormalizeRequired(
        string? value,
        string field,
        string requiredMessage,
        int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException(field, requiredMessage);
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException(
                field,
                $"Der Wert darf höchstens {maxLength} Zeichen lang sein.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string field, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException(
                field,
                $"Der Wert darf höchstens {maxLength} Zeichen lang sein.");
        }

        return normalized;
    }

    private static DateTime EnsureUtc(DateTime value, string field)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainValidationException(field, "Der Zeitstempel muss in UTC angegeben werden.");
        }

        return value;
    }
}

public sealed record CustomerData(
    string? CustomerNumber,
    string? CompanyName,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Notes);
