using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Application.CleaningTypes;

public interface ICleaningTypeRepository
{
    Task<IReadOnlyList<CleaningType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<CleaningType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken);

    Task AddAsync(CleaningType cleaningType, CancellationToken cancellationToken);

    void Remove(CleaningType cleaningType);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
