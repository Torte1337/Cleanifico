namespace Cleanifico.Application.Customers;

public sealed class CustomerConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
