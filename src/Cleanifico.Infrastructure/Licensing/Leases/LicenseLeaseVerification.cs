using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace Cleanifico.Infrastructure.Licensing.Leases;

public sealed record TrustedLicensePublicKey(
    string KeyId,
    string Algorithm,
    string SubjectPublicKeyInfoBase64);

public enum LicenseLeaseVerificationStatus
{
    Success,
    UnsupportedEnvelopeVersion,
    UnsupportedPayloadVersion,
    UnsupportedAlgorithm,
    UnknownKeyId,
    InvalidContract,
    InvalidSignature,
    InvalidPublicKey,
    ProductMismatch,
    InstallationMismatch
}

public sealed record LicenseLeaseVerificationResult(
    LicenseLeaseVerificationStatus Status,
    SignedLicenseLease? VerifiedLease)
{
    public bool Succeeded => Status == LicenseLeaseVerificationStatus.Success
        && VerifiedLease is not null;
}

public sealed class LicenseLeaseVerifier
{
    private readonly string productCode;
    private readonly Dictionary<string, TrustedKeyMaterial> trustedKeys;

    public LicenseLeaseVerifier(
        string productCode,
        IEnumerable<TrustedLicensePublicKey> trustedKeys)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        ArgumentNullException.ThrowIfNull(trustedKeys);
        if (!IsProductCode(productCode))
        {
            throw new ArgumentException("Der Produktcode ist ungültig.", nameof(productCode));
        }

        this.productCode = productCode;
        this.trustedKeys = new Dictionary<string, TrustedKeyMaterial>(StringComparer.Ordinal);
        foreach (TrustedLicensePublicKey key in trustedKeys)
        {
            if (!IsKeyId(key.KeyId)
                || !string.Equals(key.Algorithm, LicenseLeaseConstants.Algorithm, StringComparison.Ordinal)
                || !TryParseP256PublicKey(key.SubjectPublicKeyInfoBase64, out byte[] material)
                || !this.trustedKeys.TryAdd(key.KeyId, new(key.Algorithm, material)))
            {
                throw new ArgumentException("Ein vertrauenswürdiger Lizenzschlüssel ist ungültig.", nameof(trustedKeys));
            }
        }
    }

    public LicenseLeaseVerificationResult Verify(
        SignedLicenseLease lease,
        Guid expectedInstallationId)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (lease.Version != LicenseLeaseConstants.EnvelopeVersion)
        {
            return Failure(LicenseLeaseVerificationStatus.UnsupportedEnvelopeVersion);
        }

        if (lease.Payload.SchemaVersion != LicenseLeaseConstants.PayloadSchemaVersion)
        {
            return Failure(LicenseLeaseVerificationStatus.UnsupportedPayloadVersion);
        }

        if (!string.Equals(lease.Algorithm, LicenseLeaseConstants.Algorithm, StringComparison.Ordinal))
        {
            return Failure(LicenseLeaseVerificationStatus.UnsupportedAlgorithm);
        }

        if (!trustedKeys.TryGetValue(lease.KeyId, out TrustedKeyMaterial? trustedKey))
        {
            return Failure(LicenseLeaseVerificationStatus.UnknownKeyId);
        }

        if (!TryValidatePayload(lease.Payload))
        {
            return Failure(LicenseLeaseVerificationStatus.InvalidContract);
        }

        if (!string.Equals(lease.Payload.ProductCode, productCode, StringComparison.Ordinal))
        {
            return Failure(LicenseLeaseVerificationStatus.ProductMismatch);
        }

        if (expectedInstallationId == Guid.Empty
            || lease.Payload.InstallationId != expectedInstallationId)
        {
            return Failure(LicenseLeaseVerificationStatus.InstallationMismatch);
        }

        if (!TryFromBase64Url(
                lease.Signature,
                LicenseLeaseConstants.P1363SignatureLength,
                out byte[] signature))
        {
            return Failure(LicenseLeaseVerificationStatus.InvalidSignature);
        }

        byte[] signingInput = CanonicalizeSigningInput(lease);
        try
        {
            using ECDsa ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(trustedKey.SubjectPublicKeyInfo, out int bytesRead);
            if (bytesRead != trustedKey.SubjectPublicKeyInfo.Length)
            {
                return Failure(LicenseLeaseVerificationStatus.InvalidPublicKey);
            }

            bool valid = ecdsa.VerifyData(
                signingInput,
                signature,
                HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            return valid
                ? new(LicenseLeaseVerificationStatus.Success, lease)
                : Failure(LicenseLeaseVerificationStatus.InvalidSignature);
        }
        catch (CryptographicException)
        {
            return Failure(LicenseLeaseVerificationStatus.InvalidPublicKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(signingInput);
            CryptographicOperations.ZeroMemory(signature);
        }
    }

    public static byte[] CanonicalizeSigningInput(SignedLicenseLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartObject();
        writer.WriteNumber("version", lease.Version);
        writer.WriteString("keyId", lease.KeyId);
        writer.WriteString("algorithm", lease.Algorithm);
        writer.WritePropertyName("payload");
        WritePayload(writer, lease.Payload);
        writer.WriteEndObject();
        writer.Flush();
        return buffer.WrittenSpan.ToArray();
    }

    public static string ToBase64Url(ReadOnlySpan<byte> value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static void WritePayload(Utf8JsonWriter writer, LicenseLeasePayload payload)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", payload.SchemaVersion);
        writer.WriteString("leaseId", payload.LeaseId.ToString("D"));
        writer.WriteString("licenseId", payload.LicenseId.ToString("D"));
        writer.WriteString("licenseDisplayIdentifier", payload.LicenseDisplayIdentifier);
        writer.WriteString("installationId", payload.InstallationId.ToString("D"));
        writer.WriteString("productCode", payload.ProductCode);
        writer.WriteString("licenseType", payload.LicenseType.ToString());
        writer.WriteString("effectiveStatus", payload.EffectiveStatus.ToString());
        WriteUtc(writer, "issuedAtUtc", payload.IssuedAtUtc);
        WriteUtc(writer, "validUntilUtc", payload.ValidUntilUtc);
        WriteUtc(writer, "graceUntilUtc", payload.GraceUntilUtc);
        WriteNullableUtc(writer, "licenseExpiresAtUtc", payload.LicenseExpiresAtUtc);
        WriteNullableUtc(writer, "updatesUntilUtc", payload.UpdatesUntilUtc);
        writer.WritePropertyName("featureCodes");
        writer.WriteStartArray();
        foreach (string code in payload.FeatureCodes.Order(StringComparer.Ordinal).Distinct(StringComparer.Ordinal))
        {
            writer.WriteStringValue(code);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteUtc(Utf8JsonWriter writer, string name, DateTimeOffset value) =>
        writer.WriteString(name, value.ToUniversalTime().ToString(
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
            CultureInfo.InvariantCulture));

    private static void WriteNullableUtc(
        Utf8JsonWriter writer,
        string name,
        DateTimeOffset? value)
    {
        if (value is null)
        {
            writer.WriteNull(name);
        }
        else
        {
            WriteUtc(writer, name, value.Value);
        }
    }

    private static bool TryValidatePayload(LicenseLeasePayload payload)
    {
        if (payload.LeaseId == Guid.Empty
            || payload.LicenseId == Guid.Empty
            || payload.InstallationId == Guid.Empty
            || payload.LicenseDisplayIdentifier is not { Length: >= 12 and <= 36 }
            || !payload.LicenseDisplayIdentifier.StartsWith("LIC-", StringComparison.Ordinal)
            || payload.LicenseDisplayIdentifier.AsSpan(4).IndexOfAnyExcept("0123456789ABCDEF".AsSpan()) >= 0
            || !IsProductCode(payload.ProductCode)
            || !Enum.IsDefined(payload.LicenseType)
            || payload.EffectiveStatus != LicenseLeaseStatus.Active
            || payload.IssuedAtUtc.Offset != TimeSpan.Zero
            || payload.ValidUntilUtc.Offset != TimeSpan.Zero
            || payload.GraceUntilUtc.Offset != TimeSpan.Zero
            || payload.ValidUntilUtc - payload.IssuedAtUtc != LicenseLeaseConstants.LeaseLifetime
            || payload.GraceUntilUtc - payload.ValidUntilUtc != LicenseLeaseConstants.GraceLifetime
            || payload.LicenseExpiresAtUtc is { Offset: var expiresOffset } && expiresOffset != TimeSpan.Zero
            || payload.UpdatesUntilUtc is { Offset: var updatesOffset } && updatesOffset != TimeSpan.Zero
            || payload.LicenseType == LicenseLeaseType.Perpetual && payload.LicenseExpiresAtUtc is not null
            || payload.LicenseType == LicenseLeaseType.Subscription
            && (payload.LicenseExpiresAtUtc is null || payload.LicenseExpiresAtUtc <= payload.IssuedAtUtc)
            || payload.UpdatesUntilUtc is { } updatesUntil && updatesUntil <= payload.IssuedAtUtc
            || payload.FeatureCodes.Count > 512)
        {
            return false;
        }

        string? previous = null;
        foreach (string code in payload.FeatureCodes)
        {
            if (code is not { Length: >= 1 and <= 64 }
                || code.Any(character => character is not (>= 'a' and <= 'z')
                    and not (>= '0' and <= '9') and not '-' and not '_')
                || previous is not null && string.CompareOrdinal(previous, code) >= 0)
            {
                return false;
            }

            previous = code;
        }

        return true;
    }

    private static bool TryFromBase64Url(string? value, int expectedLength, out byte[] decoded)
    {
        decoded = [];
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains('=', StringComparison.Ordinal)
            || value.AsSpan().IndexOfAnyExcept(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".AsSpan()) >= 0)
        {
            return false;
        }

        string padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            0 => string.Empty,
            2 => "==",
            3 => "=",
            _ => "!"
        };

        try
        {
            decoded = Convert.FromBase64String(padded);
            return decoded.Length == expectedLength
                && string.Equals(ToBase64Url(decoded), value, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryParseP256PublicKey(string encoded, out byte[] material)
    {
        material = [];
        try
        {
            material = Convert.FromBase64String(encoded);
            using ECDsa ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(material, out int bytesRead);
            ECParameters parameters = ecdsa.ExportParameters(false);
            return bytesRead == material.Length
                && parameters.Curve.Oid.Value == "1.2.840.10045.3.1.7"
                && parameters.Q.X is { Length: 32 }
                && parameters.Q.Y is { Length: 32 };
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            material = [];
            return false;
        }
    }

    private static bool IsKeyId(string? value) =>
        value is { Length: >= 3 and <= 64 }
        && value.All(character => character is >= 'A' and <= 'Z'
            or >= 'a' and <= 'z'
            or >= '0' and <= '9'
            or '-' or '_' or '.');

    private static bool IsProductCode(string? value) =>
        value is { Length: >= 1 and <= 64 }
        && value.All(character => character is >= 'A' and <= 'Z'
            or >= '0' and <= '9'
            or '-' or '_');

    private static LicenseLeaseVerificationResult Failure(
        LicenseLeaseVerificationStatus status) => new(status, null);

    private sealed record TrustedKeyMaterial(string Algorithm, byte[] SubjectPublicKeyInfo);
}
