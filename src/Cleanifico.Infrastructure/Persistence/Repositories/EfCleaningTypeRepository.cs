using Cleanifico.Application.CleaningTypes;
using Cleanifico.Domain.CleaningTypes;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfCleaningTypeRepository(CleanificoDbContext dbContext)
    : ICleaningTypeRepository
{
    public async Task<IReadOnlyList<CleaningType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CleaningTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(cleaningType =>
                cleaningType.Name.Contains(search) || cleaningType.Code.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(cleaningType => cleaningType.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(cleaningType => cleaningType.SortOrder)
            .ThenBy(cleaningType => cleaningType.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<CleaningType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.CleaningTypes.SingleOrDefaultAsync(
            cleaningType => cleaningType.Id == id,
            cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.CleaningTypes.AnyAsync(
            cleaningType =>
                cleaningType.Name == name &&
                (!excludedId.HasValue || cleaningType.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.CleaningTypes.AnyAsync(
            cleaningType =>
                cleaningType.Code == code &&
                (!excludedId.HasValue || cleaningType.Id != excludedId.Value),
            cancellationToken);

    public async Task AddAsync(
        CleaningType cleaningType,
        CancellationToken cancellationToken) =>
        await dbContext.CleaningTypes.AddAsync(cleaningType, cancellationToken);

    public void Remove(CleaningType cleaningType) =>
        dbContext.CleaningTypes.Remove(cleaningType);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new CleaningTypeConflictException(
                "nameOrCode",
                "Name oder Kürzel des Reinigungstyps wird bereits verwendet.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1451 })
        {
            throw new CleaningTypeConflictException(
                "delete",
                "Der Reinigungstyp wird bereits verwendet und kann nicht endgültig gelöscht werden.");
        }
    }
}
