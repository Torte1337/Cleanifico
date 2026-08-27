using Cleanifico.Application.Employees;
using Cleanifico.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfEmployeeRepository(CleanificoDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        IQueryable<Employee> query = dbContext.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(employee =>
                employee.EmployeeNumber.Contains(search)
                || employee.FirstName.Contains(search)
                || employee.LastName.Contains(search)
                || (employee.Email != null && employee.Email.Contains(search))
                || (employee.Phone != null && employee.Phone.Contains(search))
                || (employee.MobilePhone != null && employee.MobilePhone.Contains(search))
                || (employee.City != null && employee.City.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(employee => employee.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.EmployeeNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.SingleOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public Task<bool> EmployeeNumberExistsAsync(
        string employeeNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(
            employee => employee.EmployeeNumber == employeeNumber
                && (!excludedId.HasValue || employee.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> HasContractsAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.EmployeeContracts.AnyAsync(contract => contract.EmployeeId == employeeId, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken) =>
        await dbContext.Employees.AddAsync(employee, cancellationToken);

    public void Remove(Employee employee) => dbContext.Employees.Remove(employee);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new EmployeeConflictException(
                "employeeNumber",
                "Die Personalnummer wird bereits verwendet.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1451 })
        {
            throw new EmployeeConflictException(
                "delete",
                "Der Mitarbeiter wird bereits verwendet und kann nicht endgültig gelöscht werden.");
        }
    }
}
