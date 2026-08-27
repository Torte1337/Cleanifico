namespace Cleanifico.Application.Employees;

public sealed record EmployeeInput(
    string? EmployeeNumber,
    string? FirstName,
    string? LastName,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Email,
    string? Phone,
    string? MobilePhone,
    DateOnly? DateOfBirth,
    string? Notes);
