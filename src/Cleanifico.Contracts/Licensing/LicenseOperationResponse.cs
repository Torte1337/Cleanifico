namespace Cleanifico.Contracts.Licensing;

public sealed record LicenseOperationResponse(
    string Status,
    bool Succeeded,
    string Message);
