namespace Cleanifico.Application.Employees;

public sealed class EmployeeNotFoundException(Guid id)
    : Exception($"Der Mitarbeiter mit der ID '{id}' wurde nicht gefunden.");

public sealed class EmployeeConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
