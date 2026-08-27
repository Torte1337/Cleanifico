using Cleanifico.Domain.Employees;

namespace Cleanifico.Application.Employees;

public sealed class EmployeeService(
    IEmployeeRepository repository,
    TimeProvider timeProvider) : IEmployeeService
{
    public Task<IReadOnlyList<Employee>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            isActive,
            cancellationToken);

    public async Task<Employee> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new EmployeeNotFoundException(id);

    public async Task<Employee> CreateAsync(
        EmployeeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var employee = Employee.Create(Guid.NewGuid(), ToData(input), UtcNow());
        await EnsureNumberUniqueAsync(employee.EmployeeNumber, null, cancellationToken);
        await repository.AddAsync(employee, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task<Employee> UpdateAsync(
        Guid id,
        EmployeeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        Employee employee = await GetByIdAsync(id, cancellationToken);
        string number = Employee.NormalizeEmployeeNumber(input.EmployeeNumber);
        Employee.NormalizeFirstName(input.FirstName);
        Employee.NormalizeLastName(input.LastName);
        await EnsureNumberUniqueAsync(number, id, cancellationToken);
        employee.Update(ToData(input), UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Employee employee = await GetByIdAsync(id, cancellationToken);
        employee.Activate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Employee employee = await GetByIdAsync(id, cancellationToken);
        employee.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Employee employee = await GetByIdAsync(id, cancellationToken);
        if (await repository.HasContractsAsync(id, cancellationToken))
        {
            throw new EmployeeConflictException(
                "delete",
                "Der Mitarbeiter besitzt Verträge und kann nicht endgültig gelöscht werden. Deaktivieren Sie ihn stattdessen.");
        }

        repository.Remove(employee);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNumberUniqueAsync(
        string number,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.EmployeeNumberExistsAsync(number, excludedId, cancellationToken))
        {
            throw new EmployeeConflictException(
                "employeeNumber",
                "Ein Mitarbeiter mit dieser Personalnummer ist bereits vorhanden.");
        }
    }

    private static EmployeeData ToData(EmployeeInput input) => new(
        input.EmployeeNumber,
        input.FirstName,
        input.LastName,
        input.Street,
        input.PostalCode,
        input.City,
        input.Country,
        input.Email,
        input.Phone,
        input.MobilePhone,
        input.DateOfBirth,
        input.Notes);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
