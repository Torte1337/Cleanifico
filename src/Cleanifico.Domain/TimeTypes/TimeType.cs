using Cleanifico.Domain.Common;

namespace Cleanifico.Domain.TimeTypes;

public sealed class TimeType
{
    public const int MaxNameLength = 200;
    public const int MaxCodeLength = 20;
    public const int MaxDescriptionLength = 1_000;
    public const int ColorLength = 7;

    private TimeType()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    private TimeType(
        Guid id,
        string name,
        string code,
        string? description,
        bool countsAsWorkTime,
        bool isPaid,
        bool requiresObject,
        bool isAbsence,
        string? color,
        int sortOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("id", "Die ID darf nicht leer sein.");
        }

        Id = id;
        Name = NormalizeName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
        CountsAsWorkTime = countsAsWorkTime;
        IsPaid = isPaid;
        RequiresObject = requiresObject;
        IsAbsence = isAbsence;
        Color = NormalizeColor(color);
        SortOrder = sortOrder;
        CreatedAtUtc = EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public string? Description { get; private set; }

    public bool CountsAsWorkTime { get; private set; }

    public bool IsPaid { get; private set; }

    public bool RequiresObject { get; private set; }

    public bool IsAbsence { get; private set; }

    public string? Color { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static TimeType Create(
        Guid id,
        string name,
        string code,
        string? description,
        bool countsAsWorkTime,
        bool isPaid,
        bool requiresObject,
        bool isAbsence,
        string? color,
        int sortOrder,
        DateTime createdAtUtc) =>
        new(
            id,
            name,
            code,
            description,
            countsAsWorkTime,
            isPaid,
            requiresObject,
            isAbsence,
            color,
            sortOrder,
            createdAtUtc);

    public void Update(
        string name,
        string code,
        string? description,
        bool countsAsWorkTime,
        bool isPaid,
        bool requiresObject,
        bool isAbsence,
        string? color,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        var normalizedName = NormalizeName(name);
        var normalizedCode = NormalizeCode(code);
        var normalizedDescription = NormalizeDescription(description);
        var normalizedColor = NormalizeColor(color);
        var validatedUpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        Name = normalizedName;
        Code = normalizedCode;
        Description = normalizedDescription;
        CountsAsWorkTime = countsAsWorkTime;
        IsPaid = isPaid;
        RequiresObject = requiresObject;
        IsAbsence = isAbsence;
        Color = normalizedColor;
        SortOrder = sortOrder;
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

    public static string NormalizeName(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException("name", "Der Name ist erforderlich.");
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new DomainValidationException(
                "name",
                $"Der Name darf höchstens {MaxNameLength} Zeichen lang sein.");
        }

        return normalized;
    }

    public static string NormalizeCode(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException("code", "Das Kürzel ist erforderlich.");
        }

        if (normalized.Length > MaxCodeLength)
        {
            throw new DomainValidationException(
                "code",
                $"Das Kürzel darf höchstens {MaxCodeLength} Zeichen lang sein.");
        }

        return normalized;
    }

    public static string? NormalizeDescription(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new DomainValidationException(
                "description",
                $"Die Beschreibung darf höchstens {MaxDescriptionLength} Zeichen lang sein.");
        }

        return normalized;
    }

    public static string? NormalizeColor(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length != ColorLength
            || normalized[0] != '#'
            || normalized.AsSpan(1).ContainsAnyExcept("0123456789ABCDEF"))
        {
            throw new DomainValidationException(
                "color",
                "Die Farbe muss als Hex-Wert im Format #RRGGBB angegeben werden.");
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
