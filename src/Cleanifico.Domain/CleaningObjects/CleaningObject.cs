using System.ComponentModel.DataAnnotations;
using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.CleaningObjects;

public sealed class CleaningObject
{
    public const int MaxObjectNumberLength = 50;
    public const int MaxNameLength = 200;
    public const int MaxStreetLength = 200;
    public const int MaxPostalCodeLength = 20;
    public const int MaxCityLength = 100;
    public const int MaxCountryLength = 100;
    public const int MaxContactNameLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 50;
    public const int MaxNotesLength = 2_000;

    private CleaningObject()
    {
        ObjectNumber = string.Empty;
        Name = string.Empty;
    }

    private CleaningObject(Guid id, CleaningObjectData data, DateTime createdAtUtc)
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
    public string ObjectNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Street { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? ContactFirstName { get; private set; }
    public string? ContactLastName { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? AccessNotes { get; private set; }
    public string? CleaningNotes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static CleaningObject Create(Guid id, CleaningObjectData data, DateTime createdAtUtc) =>
        new(id, data, createdAtUtc);

    public void Update(CleaningObjectData data, DateTime updatedAtUtc)
    {
        var normalized = Normalize(data);
        Apply(normalized);
        UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    public void Activate(DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        }
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        }
    }

    public static string NormalizeObjectNumber(string? value) =>
        NormalizeRequired(value, "objectNumber", "Die Objektnummer ist erforderlich.", MaxObjectNumberLength);

    private static CleaningObjectData Normalize(CleaningObjectData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.CustomerId == Guid.Empty)
        {
            throw new DomainValidationException("customerId", "Ein Kunde ist erforderlich.");
        }

        var email = NormalizeOptional(data.ContactEmail, "contactEmail", MaxEmailLength);
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
        {
            throw new DomainValidationException("contactEmail", "Die E-Mail-Adresse ist ungültig.");
        }

        return data with
        {
            ObjectNumber = NormalizeObjectNumber(data.ObjectNumber),
            Name = NormalizeRequired(data.Name, "name", "Der Objektname ist erforderlich.", MaxNameLength),
            Street = NormalizeOptional(data.Street, "street", MaxStreetLength),
            PostalCode = NormalizeOptional(data.PostalCode, "postalCode", MaxPostalCodeLength),
            City = NormalizeOptional(data.City, "city", MaxCityLength),
            Country = NormalizeOptional(data.Country, "country", MaxCountryLength),
            ContactFirstName = NormalizeOptional(data.ContactFirstName, "contactFirstName", MaxContactNameLength),
            ContactLastName = NormalizeOptional(data.ContactLastName, "contactLastName", MaxContactNameLength),
            ContactEmail = email,
            ContactPhone = NormalizeOptional(data.ContactPhone, "contactPhone", MaxPhoneLength),
            AccessNotes = NormalizeOptional(data.AccessNotes, "accessNotes", MaxNotesLength),
            CleaningNotes = NormalizeOptional(data.CleaningNotes, "cleaningNotes", MaxNotesLength)
        };
    }

    private void Apply(CleaningObjectData data)
    {
        ObjectNumber = data.ObjectNumber!;
        CustomerId = data.CustomerId;
        Name = data.Name!;
        Street = data.Street;
        PostalCode = data.PostalCode;
        City = data.City;
        Country = data.Country;
        ContactFirstName = data.ContactFirstName;
        ContactLastName = data.ContactLastName;
        ContactEmail = data.ContactEmail;
        ContactPhone = data.ContactPhone;
        AccessNotes = data.AccessNotes;
        CleaningNotes = data.CleaningNotes;
    }

    private static string NormalizeRequired(string? value, string field, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException(field, message);
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException(field, $"Der Wert darf höchstens {maxLength} Zeichen lang sein.");
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
            throw new DomainValidationException(field, $"Der Wert darf höchstens {maxLength} Zeichen lang sein.");
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

public sealed record CleaningObjectData(
    string? ObjectNumber,
    Guid CustomerId,
    string? Name,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? ContactFirstName,
    string? ContactLastName,
    string? ContactEmail,
    string? ContactPhone,
    string? AccessNotes,
    string? CleaningNotes);
