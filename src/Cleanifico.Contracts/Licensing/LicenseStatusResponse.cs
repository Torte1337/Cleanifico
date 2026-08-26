namespace Cleanifico.Contracts.Licensing;

public sealed record LicenseStatusResponse(
    string Status,
    bool IsValid,
    string Message,
    Guid InstallationId,
    string? LicenseDisplayIdentifier,
    DateTimeOffset? ValidUntilUtc,
    DateTimeOffset? GraceUntilUtc,
    DateTimeOffset? LastSuccessfulRefreshAtUtc,
    IReadOnlyList<string> FeatureCodes,
    string? DegradedReason);
