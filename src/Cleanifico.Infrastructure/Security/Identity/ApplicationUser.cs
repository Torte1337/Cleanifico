using Microsoft.AspNetCore.Identity;

namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public const int MaxNameLength = 100;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static ApplicationUser Create(
        Guid id,
        string firstName,
        string lastName,
        string email,
        bool isActive,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The user ID must not be empty.", nameof(id));
        }

        var validatedCreatedAtUtc = EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new ApplicationUser
        {
            Id = id,
            FirstName = NormalizeName(firstName, nameof(firstName)),
            LastName = NormalizeName(lastName, nameof(lastName)),
            Email = email,
            UserName = email,
            IsActive = isActive,
            CreatedAtUtc = validatedCreatedAtUtc,
            UpdatedAtUtc = validatedCreatedAtUtc,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        bool isActive,
        DateTime updatedAtUtc)
    {
        var normalizedFirstName = NormalizeName(firstName, nameof(firstName));
        var normalizedLastName = NormalizeName(lastName, nameof(lastName));
        var validatedUpdatedAtUtc = EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        FirstName = normalizedFirstName;
        LastName = normalizedLastName;
        IsActive = isActive;
        UpdatedAtUtc = validatedUpdatedAtUtc;
    }

    private static string NormalizeName(string? value, string field)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("The name must not be empty.", field);
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentException($"The name must not exceed {MaxNameLength} characters.", field);
        }

        return normalized;
    }

    private static DateTime EnsureUtc(DateTime value, string field)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be UTC.", field);
        }

        return value;
    }
}
