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
    DateOnly? EmploymentStartDate,
    DateOnly? EmploymentEndDate,
    string? EmploymentType,
    decimal WeeklyHours,
    decimal MonthlyTargetHours,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
