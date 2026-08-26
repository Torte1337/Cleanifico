namespace Cleanifico.Application.CleaningObjects;

public interface ICleaningObjectService
{
    Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(string? search, bool? isActive, Guid? customerId, CancellationToken cancellationToken = default);
    Task<CleaningObjectRecord> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CleaningObjectRecord> CreateAsync(CleaningObjectInput input, CancellationToken cancellationToken = default);
    Task<CleaningObjectRecord> UpdateAsync(Guid id, CleaningObjectInput input, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
