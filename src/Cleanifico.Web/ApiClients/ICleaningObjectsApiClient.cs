using Cleanifico.Contracts.CleaningObjects;

namespace Cleanifico.Web.ApiClients;

public interface ICleaningObjectsApiClient
{
    Task<IReadOnlyList<CleaningObjectResponse>> GetAllAsync(string? search, bool? isActive, Guid? customerId, CancellationToken cancellationToken = default);
    Task<CleaningObjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CleaningObjectResponse> CreateAsync(CreateCleaningObjectRequest request, CancellationToken cancellationToken = default);
    Task<CleaningObjectResponse> UpdateAsync(Guid id, UpdateCleaningObjectRequest request, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
