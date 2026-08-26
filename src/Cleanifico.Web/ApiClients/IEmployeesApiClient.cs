using Cleanifico.Contracts.Employees;

namespace Cleanifico.Web.ApiClients;

public interface IEmployeesApiClient
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);
    Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
