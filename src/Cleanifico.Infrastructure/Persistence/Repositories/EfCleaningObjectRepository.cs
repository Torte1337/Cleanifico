using Cleanifico.Application.CleaningObjects;
using Cleanifico.Domain.CleaningObjects;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfCleaningObjectRepository(CleanificoDbContext dbContext) : ICleaningObjectRepository
{
    public async Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? customerId,
        CancellationToken cancellationToken)
    {
        var query = Records();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(record =>
                record.CleaningObject.ObjectNumber.Contains(search)
                || record.CleaningObject.Name.Contains(search)
                || (record.CleaningObject.City != null && record.CleaningObject.City.Contains(search))
                || (record.CleaningObject.ContactFirstName != null && record.CleaningObject.ContactFirstName.Contains(search))
                || (record.CleaningObject.ContactLastName != null && record.CleaningObject.ContactLastName.Contains(search))
                || record.CustomerCompanyName.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(record => record.CleaningObject.IsActive == isActive.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(record => record.CleaningObject.CustomerId == customerId.Value);
        }

        return await query.OrderBy(record => record.CleaningObject.Name)
            .ThenBy(record => record.CleaningObject.ObjectNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<CleaningObjectRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Records().SingleOrDefaultAsync(record => record.CleaningObject.Id == id, cancellationToken);

    public Task<CleaningObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.CleaningObjects.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<bool> ObjectNumberExistsAsync(string objectNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        dbContext.CleaningObjects.AnyAsync(
            item => item.ObjectNumber == objectNumber && (!excludedId.HasValue || item.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(customer => customer.Id == customerId, cancellationToken);

    public async Task AddAsync(CleaningObject cleaningObject, CancellationToken cancellationToken) =>
        await dbContext.CleaningObjects.AddAsync(cleaningObject, cancellationToken);

    public void Remove(CleaningObject cleaningObject) => dbContext.CleaningObjects.Remove(cleaningObject);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new CleaningObjectConflictException("objectNumber", "Die Objektnummer wird bereits verwendet.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1452 })
        {
            throw new CleaningObjectConflictException("customerId", "Der ausgewählte Kunde wurde nicht gefunden.");
        }
    }

    private IQueryable<CleaningObjectRecord> Records() =>
        from cleaningObject in dbContext.CleaningObjects.AsNoTracking()
        join customer in dbContext.Customers.AsNoTracking() on cleaningObject.CustomerId equals customer.Id
        select new CleaningObjectRecord(cleaningObject, customer.CustomerNumber, customer.CompanyName);
}
