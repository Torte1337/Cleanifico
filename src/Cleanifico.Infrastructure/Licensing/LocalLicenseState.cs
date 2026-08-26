using System.Text.Json.Serialization;
using Cleanifico.Infrastructure.Licensing.Leases;

namespace Cleanifico.Infrastructure.Licensing;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LocalLicenseState
{
    public const int CurrentSchemaVersion = 1;

    [JsonConstructor]
    public LocalLicenseState(
        int schemaVersion,
        Guid installationId,
        string? refreshCredential = null,
        SignedLicenseLease? signedLicenseLease = null,
        DateTimeOffset? lastSuccessfulRefreshAtUtc = null)
    {
        if (schemaVersion != CurrentSchemaVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion));
        }

        if (installationId == Guid.Empty)
        {
            throw new ArgumentException("InstallationId darf nicht leer sein.", nameof(installationId));
        }

        if (refreshCredential is not null
            && !LicensingCredentialFormat.IsWellFormedRefreshCredential(refreshCredential))
        {
            throw new ArgumentException("RefreshCredential besitzt kein gültiges Format.", nameof(refreshCredential));
        }

        if (lastSuccessfulRefreshAtUtc is { Offset: var offset } && offset != TimeSpan.Zero)
        {
            throw new ArgumentException("LastSuccessfulRefreshAtUtc muss UTC verwenden.", nameof(lastSuccessfulRefreshAtUtc));
        }

        SchemaVersion = schemaVersion;
        InstallationId = installationId;
        RefreshCredential = refreshCredential;
        SignedLicenseLease = signedLicenseLease;
        LastSuccessfulRefreshAtUtc = lastSuccessfulRefreshAtUtc;
    }

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; }

    [JsonPropertyName("installationId")]
    public Guid InstallationId { get; }

    [JsonPropertyName("refreshCredential")]
    public string? RefreshCredential { get; }

    [JsonPropertyName("signedLicenseLease")]
    public SignedLicenseLease? SignedLicenseLease { get; }

    [JsonPropertyName("lastSuccessfulRefreshAtUtc")]
    public DateTimeOffset? LastSuccessfulRefreshAtUtc { get; }

    public override string ToString() =>
        $"{nameof(LocalLicenseState)} {{ SchemaVersion = {SchemaVersion}, InstallationId = {InstallationId}, RefreshCredential = [REDACTED], HasSignedLicenseLease = {SignedLicenseLease is not null}, LastSuccessfulRefreshAtUtc = {LastSuccessfulRefreshAtUtc} }}";
}

public enum LocalLicenseLoadStatus
{
    Success,
    NotFound,
    Invalid,
    Unavailable
}

public sealed record LocalLicenseLoadResult(
    LocalLicenseLoadStatus Status,
    LocalLicenseState? State)
{
    public bool Succeeded => Status == LocalLicenseLoadStatus.Success && State is not null;
}

public interface ILocalLicenseStore
{
    Task<LocalLicenseLoadResult> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(LocalLicenseState state, CancellationToken cancellationToken = default);
}

public static class LicensingCredentialFormat
{
    public const string LicenseKeyPrefix = "flk1_";
    public const string RefreshCredentialPrefix = "flr1_";
    public const int EncodedSecretLength = 43;

    public static bool IsWellFormedLicenseKey(string? value) =>
        IsWellFormed(value, LicenseKeyPrefix);

    public static bool IsWellFormedRefreshCredential(string? value) =>
        IsWellFormed(value, RefreshCredentialPrefix);

    private static bool IsWellFormed(string? value, string prefix) =>
        value is not null
        && value.Length == prefix.Length + EncodedSecretLength
        && value.StartsWith(prefix, StringComparison.Ordinal)
        && value.AsSpan(prefix.Length).IndexOfAnyExcept(
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".AsSpan()) < 0;
}
