using System.Security.Cryptography;
using Cleanifico.Infrastructure.Licensing.Leases;

namespace Cleanifico.Infrastructure.Tests;

public sealed class LicenseLeaseVerificationTests
{
    private static readonly Guid InstallationId =
        Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void SignedCleanificoLease_IsAcceptedForBoundInstallation()
    {
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SignedLicenseLease lease = CreateSignedLease(key, InstallationId, "CLEANIFICO", ["base"]);
        var verifier = CreateVerifier(key);

        LicenseLeaseVerificationResult result = verifier.Verify(lease, InstallationId);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void TamperedOrWrongInstallationLease_IsRejected()
    {
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SignedLicenseLease lease = CreateSignedLease(key, InstallationId, "CLEANIFICO", ["base"]);
        var verifier = CreateVerifier(key);

        LicenseLeaseVerificationResult result = verifier.Verify(lease, Guid.NewGuid());

        Assert.Equal(LicenseLeaseVerificationStatus.InstallationMismatch, result.Status);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void LeaseTimeEvaluator_UsesValidGraceAndExpiredStates()
    {
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SignedLicenseLease lease = CreateSignedLease(key, InstallationId, "CLEANIFICO", ["base"]);
        LicenseLeasePayload payload = lease.Payload;

        Assert.Equal(
            LicenseLeaseTimeState.Valid,
            LicenseLeaseTimeEvaluator.Evaluate(payload, payload.ValidUntilUtc));
        Assert.Equal(
            LicenseLeaseTimeState.Grace,
            LicenseLeaseTimeEvaluator.Evaluate(payload, payload.GraceUntilUtc));
        Assert.Equal(
            LicenseLeaseTimeState.Expired,
            LicenseLeaseTimeEvaluator.Evaluate(payload, payload.GraceUntilUtc.AddTicks(1)));
    }

    private static LicenseLeaseVerifier CreateVerifier(ECDsa key) => new(
        "CLEANIFICO",
        [
            new TrustedLicensePublicKey(
                "test-key-1",
                LicenseLeaseConstants.Algorithm,
                Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()))
        ]);

    private static SignedLicenseLease CreateSignedLease(
        ECDsa key,
        Guid installationId,
        string productCode,
        IReadOnlyList<string> features)
    {
        var issuedAt = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var payload = new LicenseLeasePayload(
            LicenseLeaseConstants.PayloadSchemaVersion,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "LIC-0123456789ABCDEF",
            installationId,
            productCode,
            LicenseLeaseType.Perpetual,
            LicenseLeaseStatus.Active,
            issuedAt,
            issuedAt + LicenseLeaseConstants.LeaseLifetime,
            issuedAt + LicenseLeaseConstants.LeaseLifetime + LicenseLeaseConstants.GraceLifetime,
            null,
            null,
            features);
        var unsigned = new SignedLicenseLease(
            LicenseLeaseConstants.EnvelopeVersion,
            "test-key-1",
            LicenseLeaseConstants.Algorithm,
            payload,
            new string('A', 86));
        byte[] signingInput = LicenseLeaseVerifier.CanonicalizeSigningInput(unsigned);
        byte[] signature = key.SignData(
            signingInput,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return new SignedLicenseLease(
            unsigned.Version,
            unsigned.KeyId,
            unsigned.Algorithm,
            unsigned.Payload,
            LicenseLeaseVerifier.ToBase64Url(signature));
    }
}
