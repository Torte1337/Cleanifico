using System.ComponentModel.DataAnnotations;
using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.Employees;

public sealed class Employee
{
    public const int MaxEmployeeNumberLength = 50;
    public const int MaxNameLength = 100;
    public const int MaxStreetLength = 200;
    public const int MaxPostalCodeLength = 20;
    public const int MaxCityLength = 100;
    public const int MaxCountryLength = 100;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 50;
    public const int MaxEmploymentTypeLength = 100;
    public const int MaxNotesLength = 2_000;

    private Employee()
    {
        EmployeeNumber = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private Employee(Guid id, EmployeeData data, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("id", "Die ID darf nicht leer sein.");
        }

        Apply(Normalize(data));
        Id = id;
        IsActive = true;
        CreatedAtUtc = EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string EmployeeNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Street { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? MobilePhone { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public DateOnly? EmploymentStartDate { get; private set; }
    public DateOnly? EmploymentEndDate { get; private set; }
    public string? EmploymentType { get; private set; }
    public decimal WeeklyHours { get; private set; }
    public decimal MonthlyTargetHours { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Employee Create(Guid id, EmployeeData data, DateTime createdAtUtc) =>
        new(id, data, createdAtUtc);

    public void Update(EmployeeData data, DateTime updatedAtUtc)
    {
        Apply(Normalize(data));
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

    public static string NormalizeEmployeeNumber(string? value) =>
        NormalizeRequired(
            value,
            "employeeNumber",
            "Die Personalnummer ist erforderlich.",
            MaxEmployeeNumberLength);

    public static string NormalizeFirstName(string? value) =>
        NormalizeRequired(value, "firstName", "Der Vorname ist erforderlich.", MaxNameLength);

    public static string NormalizeLastName(string? value) =>
        NormalizeRequired(value, "lastName", "Der Nachname ist erforderlich.", MaxNameLength);

    private static EmployeeData Normalize(EmployeeData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.EmploymentStartDate is { } start
            && data.EmploymentEndDate is { } end
            && end < start)
        {
            throw new DomainValidationException(
                "employmentEndDate",
                "Das Beschäftigungsende darf nicht vor dem Beschäftigungsbeginn liegen.");
        }

        if (data.WeeklyHours < 0)
        {
            throw new DomainValidationException(
                "weeklyHours",
                "Die Wochenstunden dürfen nicht negativ sein.");
        }

        if (data.MonthlyTargetHours < 0)
        {
            throw new DomainValidationException(
                "monthlyTargetHours",
                "Die monatlichen Sollstunden dürfen nicht negativ sein.");
        }

        string? email = NormalizeOptional(data.Email, "email", MaxEmailLength);
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
        {
            throw new DomainValidationException("email", "Die E-Mail-Adresse ist ungültig.");
        }

        return data with
        {
            EmployeeNumber = NormalizeEmployeeNumber(data.EmployeeNumber),
            FirstName = NormalizeFirstName(data.FirstName),
            LastName = NormalizeLastName(data.LastName),
            Street = NormalizeOptional(data.Street, "street", MaxStreetLength),
            PostalCode = NormalizeOptional(data.PostalCode, "postalCode", MaxPostalCodeLength),
            City = NormalizeOptional(data.City, "city", MaxCityLength),
            Country = NormalizeOptional(data.Country, "country", MaxCountryLength),
            Email = email,
            Phone = NormalizeOptional(data.Phone, "phone", MaxPhoneLength),
            MobilePhone = NormalizeOptional(data.MobilePhone, "mobilePhone", MaxPhoneLength),
            EmploymentType = NormalizeOptional(
                data.EmploymentType,
                "employmentType",
                MaxEmploymentTypeLength),
            Notes = NormalizeOptional(data.Notes, "notes", MaxNotesLength)
        };
    }

    private void Apply(EmployeeData data)
    {
        EmployeeNumber = data.EmployeeNumber!;
        FirstName = data.FirstName!;
        LastName = data.LastName!;
        Street = data.Street;
        PostalCode = data.PostalCode;
        City = data.City;
        Country = data.Country;
        Email = data.Email;
        Phone = data.Phone;
        MobilePhone = data.MobilePhone;
        DateOfBirth = data.DateOfBirth;
        EmploymentStartDate = data.EmploymentStartDate;
        EmploymentEndDate = data.EmploymentEndDate;
        EmploymentType = data.EmploymentType;
        WeeklyHours = data.WeeklyHours;
        MonthlyTargetHours = data.MonthlyTargetHours;
        Notes = data.Notes;
    }

    private static string NormalizeRequired(
        string? value,
        string field,
        string message,
        int maxLength)
    {
        string? normalized = value?.Trim();
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
        string? normalized = value?.Trim();
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

public sealed record EmployeeData(
    string? EmployeeNumber,
    string? FirstName,
    string? LastName,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Email,
    string? Phone,
    string? MobilePhone,
    DateOnly? DateOfBirth,
    DateOnly? EmploymentStartDate,
    DateOnly? EmploymentEndDate,
    string? EmploymentType,
    decimal WeeklyHours,
    decimal MonthlyTargetHours,
    string? Notes);
