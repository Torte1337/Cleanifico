using Cleanifico.Domain.EmployeeContracts;

namespace Cleanifico.Application.EmployeeContracts;

public interface IEmployeeContractRepository
{
    Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken);

    Task<EmployeeContractRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken);
    Task<bool> ContractNumberExistsAsync(
        string contractNumber,
        Guid? excludedId,
        CancellationToken cancellationToken);
    Task<bool> HasOverlappingActiveContractAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        Guid? excludedId,
        CancellationToken cancellationToken);
    Task AddAsync(EmployeeContract contract, CancellationToken cancellationToken);
    void Remove(EmployeeContract contract);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
