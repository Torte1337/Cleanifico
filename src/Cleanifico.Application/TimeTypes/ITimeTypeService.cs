using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Application.TimeTypes;

public interface ITimeTypeService
{
    Task<IReadOnlyList<TimeType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<TimeType> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TimeType> CreateAsync(TimeTypeInput input, CancellationToken cancellationToken = default);

    Task<TimeType> UpdateAsync(Guid id, TimeTypeInput input, CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
