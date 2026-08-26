using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Cleanifico.Infrastructure.Licensing.Leases;

[JsonConverter(typeof(JsonStringEnumConverter<LicenseLeaseType>))]
public enum LicenseLeaseType
{
    Perpetual,
    Subscription
}

[JsonConverter(typeof(JsonStringEnumConverter<LicenseLeaseStatus>))]
public enum LicenseLeaseStatus
{
    Active,
    Suspended,
    Revoked,
    Expired
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LicenseLeasePayload
{
    [JsonConstructor]
    public LicenseLeasePayload(
        int schemaVersion,
        Guid leaseId,
        Guid licenseId,
        string licenseDisplayIdentifier,
        Guid installationId,
        string productCode,
        LicenseLeaseType licenseType,
        LicenseLeaseStatus effectiveStatus,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset validUntilUtc,
        DateTimeOffset graceUntilUtc,
        DateTimeOffset? licenseExpiresAtUtc,
        DateTimeOffset? updatesUntilUtc,
        IReadOnlyList<string> featureCodes)
    {
        SchemaVersion = schemaVersion;
        LeaseId = leaseId;
        LicenseId = licenseId;
        LicenseDisplayIdentifier = licenseDisplayIdentifier
            ?? throw new ArgumentNullException(nameof(licenseDisplayIdentifier));
        InstallationId = installationId;
        ProductCode = productCode ?? throw new ArgumentNullException(nameof(productCode));
        LicenseType = licenseType;
        EffectiveStatus = effectiveStatus;
        IssuedAtUtc = issuedAtUtc;
        ValidUntilUtc = validUntilUtc;
        GraceUntilUtc = graceUntilUtc;
        LicenseExpiresAtUtc = licenseExpiresAtUtc;
        UpdatesUntilUtc = updatesUntilUtc;
        FeatureCodes = new ReadOnlyCollection<string>(
            (featureCodes ?? throw new ArgumentNullException(nameof(featureCodes))).ToArray());
    }

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; }

    [JsonPropertyName("leaseId")]
    public Guid LeaseId { get; }

    [JsonPropertyName("licenseId")]
    public Guid LicenseId { get; }

    [JsonPropertyName("licenseDisplayIdentifier")]
    public string LicenseDisplayIdentifier { get; }

    [JsonPropertyName("installationId")]
    public Guid InstallationId { get; }

    [JsonPropertyName("productCode")]
    public string ProductCode { get; }

    [JsonPropertyName("licenseType")]
    public LicenseLeaseType LicenseType { get; }

    [JsonPropertyName("effectiveStatus")]
    public LicenseLeaseStatus EffectiveStatus { get; }

    [JsonPropertyName("issuedAtUtc")]
    public DateTimeOffset IssuedAtUtc { get; }

    [JsonPropertyName("validUntilUtc")]
    public DateTimeOffset ValidUntilUtc { get; }

    [JsonPropertyName("graceUntilUtc")]
    public DateTimeOffset GraceUntilUtc { get; }

    [JsonPropertyName("licenseExpiresAtUtc")]
    public DateTimeOffset? LicenseExpiresAtUtc { get; }

    [JsonPropertyName("updatesUntilUtc")]
    public DateTimeOffset? UpdatesUntilUtc { get; }

    [JsonPropertyName("featureCodes")]
    public IReadOnlyList<string> FeatureCodes { get; }

    public override string ToString() =>
        $"{nameof(LicenseLeasePayload)} {{ LeaseId = {LeaseId}, LicenseId = {LicenseId}, InstallationId = {InstallationId}, ProductCode = {ProductCode} }}";
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SignedLicenseLease
{
    [JsonConstructor]
    public SignedLicenseLease(
        int version,
        string keyId,
        string algorithm,
        LicenseLeasePayload payload,
        string signature)
    {
        Version = version;
        KeyId = keyId ?? throw new ArgumentNullException(nameof(keyId));
        Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Signature = signature ?? throw new ArgumentNullException(nameof(signature));
    }

    [JsonPropertyName("version")]
    public int Version { get; }

    [JsonPropertyName("keyId")]
    public string KeyId { get; }

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; }

    [JsonPropertyName("payload")]
    public LicenseLeasePayload Payload { get; }

    [JsonPropertyName("signature")]
    public string Signature { get; }

    public override string ToString() =>
        $"{nameof(SignedLicenseLease)} {{ Version = {Version}, KeyId = {KeyId}, Algorithm = {Algorithm}, LeaseId = {Payload.LeaseId} }}";
}

public static class LicenseLeaseConstants
{
    public const int EnvelopeVersion = 1;
    public const int PayloadSchemaVersion = 1;
    public const string Algorithm = "ECDSA-P256-SHA256";
    public const int P1363SignatureLength = 64;
    public static readonly TimeSpan LeaseLifetime = TimeSpan.FromDays(30);
    public static readonly TimeSpan GraceLifetime = TimeSpan.FromDays(14);
}

public enum LicenseLeaseTimeState
{
    Valid,
    Grace,
    Expired,
    NotYetValid,
    InvalidTimeRange
}

public static class LicenseLeaseTimeEvaluator
{
    public static LicenseLeaseTimeState Evaluate(
        LicenseLeasePayload payload,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.IssuedAtUtc.Offset != TimeSpan.Zero
            || payload.ValidUntilUtc.Offset != TimeSpan.Zero
            || payload.GraceUntilUtc.Offset != TimeSpan.Zero
            || payload.IssuedAtUtc >= payload.ValidUntilUtc
            || payload.ValidUntilUtc > payload.GraceUntilUtc)
        {
            return LicenseLeaseTimeState.InvalidTimeRange;
        }

        DateTimeOffset utcNow = now.ToUniversalTime();
        if (utcNow < payload.IssuedAtUtc)
        {
            return LicenseLeaseTimeState.NotYetValid;
        }

        if (utcNow <= payload.ValidUntilUtc)
        {
            return LicenseLeaseTimeState.Valid;
        }

        return utcNow <= payload.GraceUntilUtc
            ? LicenseLeaseTimeState.Grace
            : LicenseLeaseTimeState.Expired;
    }
}
