using Cleanifico.Contracts.TimeTypes;

namespace Cleanifico.Web.ApiClients;

public interface ITimeTypesApiClient
{
    Task<IReadOnlyList<TimeTypeResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<TimeTypeResponse> CreateAsync(
        CreateTimeTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<TimeTypeResponse> UpdateAsync(
        Guid id,
        UpdateTimeTypeRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
