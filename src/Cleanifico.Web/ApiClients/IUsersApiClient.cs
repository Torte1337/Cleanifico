using Cleanifico.Contracts.Users;

namespace Cleanifico.Web.ApiClients;

public interface IUsersApiClient
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateRolesAsync(
        Guid id,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
