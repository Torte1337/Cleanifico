namespace Cleanifico.Contracts.Employees;

public sealed record EmployeeResponse(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Email,
    string? Phone,
    string? MobilePhone,
    DateOnly? DateOfBirth,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
