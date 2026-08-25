using Cleanifico.Application.TimeTypes;
using Cleanifico.Domain.TimeTypes;
using Cleanifico.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Cleanifico.Infrastructure.Persistence.Repositories;

public sealed class EfTimeTypeRepository(CleanificoDbContext dbContext) : ITimeTypeRepository
{
    private const string InitializationKey = "TimeTypes.StandardData.v1";

    public async Task<IReadOnlyList<TimeType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.TimeTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(timeType =>
                timeType.Name.Contains(search) || timeType.Code.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(timeType => timeType.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(timeType => timeType.SortOrder)
            .ThenBy(timeType => timeType.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<TimeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TimeTypes.SingleOrDefaultAsync(timeType => timeType.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.TimeTypes.AnyAsync(
            timeType => timeType.Name == name
                && (!excludedId.HasValue || timeType.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        dbContext.TimeTypes.AnyAsync(
            timeType => timeType.Code == code
                && (!excludedId.HasValue || timeType.Id != excludedId.Value),
            cancellationToken);

    public async Task AddAsync(TimeType timeType, CancellationToken cancellationToken) =>
        await dbContext.TimeTypes.AddAsync(timeType, cancellationToken);

    public void Remove(TimeType timeType) => dbContext.TimeTypes.Remove(timeType);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new TimeTypeConflictException(
                "nameOrCode",
                "Name oder Kürzel des Zeittyps wird bereits verwendet.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is MySqlException { Number: 1451 })
        {
            throw new TimeTypeConflictException(
                "delete",
                "Der Zeittyp wird bereits verwendet und kann nicht endgültig gelöscht werden.");
        }
    }

    public async Task InitializeDefaultsAsync(
        IReadOnlyCollection<TimeType> defaults,
        DateTime initializedAtUtc,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (await dbContext.DataInitializationMarkers.AnyAsync(
                    marker => marker.Key == InitializationKey,
                    cancellationToken))
            {
                return;
            }

            await dbContext.TimeTypes.AddRangeAsync(defaults, cancellationToken);
            await dbContext.DataInitializationMarkers.AddAsync(
                DataInitializationMarker.Create(InitializationKey, initializedAtUtc),
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
