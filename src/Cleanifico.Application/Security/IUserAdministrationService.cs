namespace Cleanifico.Application.Security;

public interface IUserAdministrationService
{
    Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserAccount> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAccount> CreateAsync(CreateUserInput input, CancellationToken cancellationToken = default);
    Task<UserAccount> UpdateAsync(Guid id, UpdateUserInput input, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<UserAccount> UpdateRolesAsync(
        Guid id,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
