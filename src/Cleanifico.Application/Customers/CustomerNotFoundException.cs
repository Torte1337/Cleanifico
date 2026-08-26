namespace Cleanifico.Application.Customers;

public sealed class CustomerNotFoundException(Guid id)
    : Exception($"Der Kunde mit der ID '{id}' wurde nicht gefunden.");
