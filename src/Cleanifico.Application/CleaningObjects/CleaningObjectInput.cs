namespace Cleanifico.Application.CleaningObjects;

public sealed record CleaningObjectInput(
    string? ObjectNumber,
    Guid CustomerId,
    string? Name,
    string? Street,
    string? PostalCode,
    string? City,
    string? Country,
    string? ContactFirstName,
    string? ContactLastName,
    string? ContactEmail,
    string? ContactPhone,
    string? AccessNotes,
    string? CleaningNotes);
