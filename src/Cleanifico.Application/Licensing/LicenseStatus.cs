namespace Cleanifico.Application.Licensing;

public enum LicenseStatus
{
    NotActivated,
    Valid,
    Grace,
    Expired,
    Invalid
}

public sealed record LicenseCheckResult(
    LicenseStatus Status,
    Guid InstallationId = default,
    string? LicenseDisplayIdentifier = null,
    DateTimeOffset? ValidUntilUtc = null,
    DateTimeOffset? GraceUntilUtc = null,
    DateTimeOffset? LastSuccessfulRefreshAtUtc = null,
    IReadOnlyList<string>? FeatureCodes = null,
    string? DegradedReason = null)
{
    public bool IsValid => Status is LicenseStatus.Valid or LicenseStatus.Grace;

    public IReadOnlyList<string> EffectiveFeatureCodes => FeatureCodes ?? [];
}
