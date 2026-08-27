using Cleanifico.Domain.Employees;

namespace Cleanifico.Application.Employees;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmployeeNumberExistsAsync(
        string employeeNumber,
        Guid? excludedId,
        CancellationToken cancellationToken);

    Task<bool> HasContractsAsync(Guid employeeId, CancellationToken cancellationToken);

    Task AddAsync(Employee employee, CancellationToken cancellationToken);

    void Remove(Employee employee);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
