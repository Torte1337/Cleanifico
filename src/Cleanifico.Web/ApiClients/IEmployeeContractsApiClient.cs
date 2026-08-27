using Cleanifico.Contracts.EmployeeContracts;

namespace Cleanifico.Web.ApiClients;

public interface IEmployeeContractsApiClient
{
    Task<IReadOnlyList<EmployeeContractResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken = default);
    Task<EmployeeContractResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<EmployeeContractResponse> CreateAsync(
        CreateEmployeeContractRequest request,
        CancellationToken cancellationToken = default);
    Task<EmployeeContractResponse> UpdateAsync(
        Guid id,
        UpdateEmployeeContractRequest request,
        CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
