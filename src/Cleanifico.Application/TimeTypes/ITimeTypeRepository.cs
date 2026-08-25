using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Application.TimeTypes;

public interface ITimeTypeRepository
{
    Task<IReadOnlyList<TimeType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<TimeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? excludedId, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, Guid? excludedId, CancellationToken cancellationToken);

    Task AddAsync(TimeType timeType, CancellationToken cancellationToken);

    void Remove(TimeType timeType);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task InitializeDefaultsAsync(
        IReadOnlyCollection<TimeType> defaults,
        DateTime initializedAtUtc,
        CancellationToken cancellationToken);
}
