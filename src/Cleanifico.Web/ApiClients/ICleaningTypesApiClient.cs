using Cleanifico.Contracts.CleaningTypes;

namespace Cleanifico.Web.ApiClients;

public interface ICleaningTypesApiClient
{
    Task<IReadOnlyList<CleaningTypeResponse>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<CleaningTypeResponse> CreateAsync(
        CreateCleaningTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<CleaningTypeResponse> UpdateAsync(
        Guid id,
        UpdateCleaningTypeRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
