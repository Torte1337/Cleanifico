using Cleanifico.Domain.CleaningObjects;

namespace Cleanifico.Application.CleaningObjects;

public interface ICleaningObjectRepository
{
    Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? customerId,
        CancellationToken cancellationToken);

    Task<CleaningObjectRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CleaningObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ObjectNumberExistsAsync(string objectNumber, Guid? excludedId, CancellationToken cancellationToken);

    Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken);

    Task AddAsync(CleaningObject cleaningObject, CancellationToken cancellationToken);

    void Remove(CleaningObject cleaningObject);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
