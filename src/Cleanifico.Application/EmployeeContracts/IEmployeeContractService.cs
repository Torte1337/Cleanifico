namespace Cleanifico.Application.EmployeeContracts;

public interface IEmployeeContractService
{
    Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken = default);
    Task<EmployeeContractRecord> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeContractRecord> CreateAsync(
        EmployeeContractInput input,
        CancellationToken cancellationToken = default);
    Task<EmployeeContractRecord> UpdateAsync(
        Guid id,
        EmployeeContractInput input,
        CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
