using Cleanifico.Application.Customers;
using Cleanifico.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfCustomerRepository(CleanificoDbContext dbContext) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(customer =>
                customer.CustomerNumber.Contains(search)
                || customer.CompanyName.Contains(search)
                || (customer.ContactFirstName != null && customer.ContactFirstName.Contains(search))
                || (customer.ContactLastName != null && customer.ContactLastName.Contains(search))
                || (customer.City != null && customer.City.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(customer => customer.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(customer => customer.CompanyName)
            .ThenBy(customer => customer.CustomerNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<bool> CustomerNumberExistsAsync(
        string customerNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(
            customer => customer.CustomerNumber == customerNumber
                && (!excludedId.HasValue || customer.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> HasCleaningObjectsAsync(Guid customerId, CancellationToken cancellationToken) =>
        dbContext.CleaningObjects.AnyAsync(item => item.CustomerId == customerId, cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken) =>
        await dbContext.Customers.AddAsync(customer, cancellationToken);

    public void Remove(Customer customer) => dbContext.Customers.Remove(customer);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new CustomerConflictException(
                "customerNumber",
                "Die Kundennummer wird bereits verwendet.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1451 })
        {
            throw new CustomerConflictException(
                "delete",
                "Der Kunde wird bereits verwendet und kann nicht endgültig gelöscht werden.");
        }
    }
}
