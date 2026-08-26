using Cleanifico.Domain.Employees;

namespace Cleanifico.Application.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<Employee>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<Employee> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee> CreateAsync(EmployeeInput input, CancellationToken cancellationToken = default);
    Task<Employee> UpdateAsync(Guid id, EmployeeInput input, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
