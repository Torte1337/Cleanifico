using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Application.CleaningTypes;

public interface ICleaningTypeService
{
    Task<IReadOnlyList<CleaningType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<CleaningType> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CleaningType> CreateAsync(
        CleaningTypeInput input,
        CancellationToken cancellationToken = default);

    Task<CleaningType> UpdateAsync(
        Guid id,
        CleaningTypeInput input,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
