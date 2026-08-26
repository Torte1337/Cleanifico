namespace Cleanifico.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    string CustomerNumber,
    string CompanyName,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
