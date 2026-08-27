using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.EmployeeContracts;

public sealed class EmployeeContract
{
    public const int MaxContractNumberLength = 50;
    public const int MaxEmploymentTypeLength = 100;
    public const int MaxNotesLength = 2_000;

    private EmployeeContract()
    {
        ContractNumber = string.Empty;
    }

    private EmployeeContract(Guid id, EmployeeContractData data, DateTime createdAtUtc)
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
    public string ContractNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsPermanent { get; private set; }
    public string? EmploymentType { get; private set; }
    public decimal WeeklyHours { get; private set; }
    public decimal MonthlyTargetHours { get; private set; }
    public decimal VacationDaysPerYear { get; private set; }
    public DateOnly? ProbationEndDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static EmployeeContract Create(Guid id, EmployeeContractData data, DateTime createdAtUtc) =>
        new(id, data, createdAtUtc);

    public void Update(EmployeeContractData data, DateTime updatedAtUtc)
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

    public static string NormalizeContractNumber(string? value) =>
        NormalizeRequired(
            value,
            "contractNumber",
            "Die Vertragsnummer ist erforderlich.",
            MaxContractNumberLength);

    private static EmployeeContractData Normalize(EmployeeContractData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.EmployeeId == Guid.Empty)
        {
            throw new DomainValidationException("employeeId", "Ein Mitarbeiter ist erforderlich.");
        }

        if (data.StartDate == default)
        {
            throw new DomainValidationException("startDate", "Der Vertragsbeginn ist erforderlich.");
        }

        if (data.IsPermanent && data.EndDate.HasValue)
        {
            throw new DomainValidationException(
                "endDate",
                "Ein unbefristeter Vertrag darf kein reguläres Enddatum besitzen.");
        }

        if (data.EndDate is { } endDate && endDate < data.StartDate)
        {
            throw new DomainValidationException(
                "endDate",
                "Das Vertragsende darf nicht vor dem Vertragsbeginn liegen.");
        }

        if (data.ProbationEndDate is { } probationEndDate && probationEndDate < data.StartDate)
        {
            throw new DomainValidationException(
                "probationEndDate",
                "Das Ende der Probezeit darf nicht vor dem Vertragsbeginn liegen.");
        }

        EnsureNonNegative(data.WeeklyHours, "weeklyHours", "Die Wochenstunden dürfen nicht negativ sein.");
        EnsureNonNegative(
            data.MonthlyTargetHours,
            "monthlyTargetHours",
            "Die monatlichen Sollstunden dürfen nicht negativ sein.");
        EnsureNonNegative(
            data.VacationDaysPerYear,
            "vacationDaysPerYear",
            "Die Urlaubstage dürfen nicht negativ sein.");

        return data with
        {
            ContractNumber = NormalizeContractNumber(data.ContractNumber),
            EmploymentType = NormalizeOptional(
                data.EmploymentType,
                "employmentType",
                MaxEmploymentTypeLength),
            Notes = NormalizeOptional(data.Notes, "notes", MaxNotesLength)
        };
    }

    private void Apply(EmployeeContractData data)
    {
        ContractNumber = data.ContractNumber!;
        EmployeeId = data.EmployeeId;
        StartDate = data.StartDate;
        EndDate = data.EndDate;
        IsPermanent = data.IsPermanent;
        EmploymentType = data.EmploymentType;
        WeeklyHours = data.WeeklyHours;
        MonthlyTargetHours = data.MonthlyTargetHours;
        VacationDaysPerYear = data.VacationDaysPerYear;
        ProbationEndDate = data.ProbationEndDate;
        Notes = data.Notes;
    }

    private static void EnsureNonNegative(decimal value, string field, string message)
    {
        if (value < 0)
        {
            throw new DomainValidationException(field, message);
        }
    }

    private static string NormalizeRequired(string? value, string field, string message, int maxLength)
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

public sealed record EmployeeContractData(
    string? ContractNumber,
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsPermanent,
    string? EmploymentType,
    decimal WeeklyHours,
    decimal MonthlyTargetHours,
    decimal VacationDaysPerYear,
    DateOnly? ProbationEndDate,
    string? Notes);
