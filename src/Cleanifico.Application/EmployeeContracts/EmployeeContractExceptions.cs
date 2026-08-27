namespace Cleanifico.Application.EmployeeContracts;

public sealed class EmployeeContractNotFoundException(Guid id)
    : Exception($"Der Mitarbeitervertrag mit der ID '{id}' wurde nicht gefunden.");

public sealed class EmployeeContractConflictException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
