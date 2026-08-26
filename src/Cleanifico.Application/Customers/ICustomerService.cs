using Cleanifico.Domain.Customers;

namespace Cleanifico.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<Customer> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer> CreateAsync(CustomerInput input, CancellationToken cancellationToken = default);

    Task<Customer> UpdateAsync(
        Guid id,
        CustomerInput input,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
