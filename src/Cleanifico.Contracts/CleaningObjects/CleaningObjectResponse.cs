namespace Cleanifico.Contracts.CleaningObjects;

public sealed record CleaningObjectResponse(
    Guid Id,
    string ObjectNumber,
    Guid CustomerId,
    string CustomerNumber,
    string CustomerCompanyName,
    string Name,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? ContactFirstName,
    string? ContactLastName,
    string? ContactEmail,
    string? ContactPhone,
    string? AccessNotes,
    string? CleaningNotes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
