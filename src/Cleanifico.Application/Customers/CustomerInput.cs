namespace Cleanifico.Application.Customers;

public sealed record CustomerInput(
    string? CustomerNumber,
    string? CompanyName,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? Notes);
