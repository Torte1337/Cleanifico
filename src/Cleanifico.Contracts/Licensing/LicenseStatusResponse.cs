namespace Cleanifico.Contracts.Licensing;

public sealed record LicenseStatusResponse(
    string Status,
    bool IsValid,
    string Message);
