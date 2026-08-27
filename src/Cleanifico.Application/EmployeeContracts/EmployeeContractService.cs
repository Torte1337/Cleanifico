using Cleanifico.Domain.Common;
using Cleanifico.Domain.EmployeeContracts;

namespace Cleanifico.Application.EmployeeContracts;

public sealed class EmployeeContractService(
    IEmployeeContractRepository repository,
    TimeProvider timeProvider) : IEmployeeContractService
{
    public Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            isActive,
            employeeId,
            cancellationToken);

    public async Task<EmployeeContractRecord> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await repository.GetRecordByIdAsync(id, cancellationToken)
        ?? throw new EmployeeContractNotFoundException(id);

    public async Task<EmployeeContractRecord> CreateAsync(
        EmployeeContractInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await EnsureEmployeeExistsAsync(input.EmployeeId, cancellationToken);
        EmployeeContract contract = EmployeeContract.Create(Guid.NewGuid(), ToData(input), UtcNow());
        await EnsureNumberUniqueAsync(contract.ContractNumber, null, cancellationToken);
        await EnsureNoOverlapAsync(contract.EmployeeId, contract.StartDate, contract.EndDate, null, cancellationToken);
        await repository.AddAsync(contract, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(contract.Id, cancellationToken);
    }

    public async Task<EmployeeContractRecord> UpdateAsync(
        Guid id,
        EmployeeContractInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        EmployeeContract contract = await GetEntityAsync(id, cancellationToken);
        await EnsureEmployeeExistsAsync(input.EmployeeId, cancellationToken);
        string number = EmployeeContract.NormalizeContractNumber(input.ContractNumber);
        EmployeeContractData data = ToData(input);
        EmployeeContract validationContract = EmployeeContract.Create(Guid.NewGuid(), data, UtcNow());
        await EnsureNumberUniqueAsync(number, id, cancellationToken);
        if (contract.IsActive)
        {
            await EnsureNoOverlapAsync(
                input.EmployeeId,
                validationContract.StartDate,
                validationContract.EndDate,
                id,
                cancellationToken);
        }

        contract.Update(data, UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmployeeContract contract = await GetEntityAsync(id, cancellationToken);
        if (!contract.IsActive)
        {
            await EnsureNoOverlapAsync(
                contract.EmployeeId,
                contract.StartDate,
                contract.EndDate,
                contract.Id,
                cancellationToken);
            contract.Activate(UtcNow());
            await repository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmployeeContract contract = await GetEntityAsync(id, cancellationToken);
        contract.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmployeeContract contract = await GetEntityAsync(id, cancellationToken);
        repository.Remove(contract);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<EmployeeContract> GetEntityAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new EmployeeContractNotFoundException(id);

    private async Task EnsureEmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        if (employeeId == Guid.Empty)
        {
            throw new DomainValidationException("employeeId", "Ein Mitarbeiter ist erforderlich.");
        }

        if (!await repository.EmployeeExistsAsync(employeeId, cancellationToken))
        {
            throw new DomainValidationException("employeeId", "Der ausgewählte Mitarbeiter wurde nicht gefunden.");
        }
    }

    private async Task EnsureNumberUniqueAsync(
        string contractNumber,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.ContractNumberExistsAsync(contractNumber, excludedId, cancellationToken))
        {
            throw new EmployeeContractConflictException(
                "contractNumber",
                "Ein Vertrag mit dieser Vertragsnummer ist bereits vorhanden.");
        }
    }

    private async Task EnsureNoOverlapAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.HasOverlappingActiveContractAsync(
                employeeId,
                startDate,
                endDate,
                excludedId,
                cancellationToken))
        {
            throw new EmployeeContractConflictException(
                "startDate",
                "Der Vertragszeitraum überschneidet sich mit einem anderen aktiven Vertrag dieses Mitarbeiters.");
        }
    }

    private static EmployeeContractData ToData(EmployeeContractInput input) => new(
        input.ContractNumber,
        input.EmployeeId,
        input.StartDate,
        input.EndDate,
        input.IsPermanent,
        input.EmploymentType,
        input.WeeklyHours,
        input.MonthlyTargetHours,
        input.VacationDaysPerYear,
        input.ProbationEndDate,
        input.Notes);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
