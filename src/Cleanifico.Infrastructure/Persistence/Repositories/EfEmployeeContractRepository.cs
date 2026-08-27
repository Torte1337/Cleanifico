using Cleanifico.Application.EmployeeContracts;
using Cleanifico.Domain.EmployeeContracts;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfEmployeeContractRepository(CleanificoDbContext dbContext)
    : IEmployeeContractRepository
{
    public async Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken)
    {
        IQueryable<EmployeeContractRecord> query = Records();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(record =>
                record.Contract.ContractNumber.Contains(search)
                || (record.Contract.EmploymentType != null
                    && record.Contract.EmploymentType.Contains(search))
                || record.EmployeeNumber.Contains(search)
                || record.EmployeeName.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(record => record.Contract.IsActive == isActive.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(record => record.Contract.EmployeeId == employeeId.Value);
        }

        return await query
            .OrderByDescending(record => record.Contract.StartDate)
            .ThenBy(record => record.Contract.ContractNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeContractRecord?> GetRecordByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        Records().SingleOrDefaultAsync(record => record.Contract.Id == id, cancellationToken);

    public Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.EmployeeContracts.SingleOrDefaultAsync(contract => contract.Id == id, cancellationToken);

    public Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(employee => employee.Id == employeeId, cancellationToken);

    public Task<bool> ContractNumberExistsAsync(
        string contractNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.EmployeeContracts.AnyAsync(
            contract => contract.ContractNumber == contractNumber
                && (!excludedId.HasValue || contract.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> HasOverlappingActiveContractAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.EmployeeContracts.AnyAsync(
            contract => contract.EmployeeId == employeeId
                && contract.IsActive
                && (!excludedId.HasValue || contract.Id != excludedId.Value)
                && (!endDate.HasValue || contract.StartDate <= endDate.Value)
                && (!contract.EndDate.HasValue || startDate <= contract.EndDate.Value),
            cancellationToken);

    public async Task AddAsync(EmployeeContract contract, CancellationToken cancellationToken) =>
        await dbContext.EmployeeContracts.AddAsync(contract, cancellationToken);

    public void Remove(EmployeeContract contract) => dbContext.EmployeeContracts.Remove(contract);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new EmployeeContractConflictException(
                "contractNumber",
                "Die Vertragsnummer wird bereits verwendet.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1452 })
        {
            throw new EmployeeContractConflictException(
                "employeeId",
                "Der ausgewählte Mitarbeiter wurde nicht gefunden.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1451 })
        {
            throw new EmployeeContractConflictException(
                "delete",
                "Der Vertrag wird bereits historisch referenziert und kann nicht endgültig gelöscht werden.");
        }
    }

    private IQueryable<EmployeeContractRecord> Records() =>
        from contract in dbContext.EmployeeContracts.AsNoTracking()
        join employee in dbContext.Employees.AsNoTracking() on contract.EmployeeId equals employee.Id
        select new EmployeeContractRecord(
            contract,
            employee.EmployeeNumber,
            employee.FirstName + " " + employee.LastName);
}
