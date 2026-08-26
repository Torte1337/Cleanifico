using Cleanifico.Domain.Customers;

namespace Cleanifico.Application.Customers;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CustomerNumberExistsAsync(
        string customerNumber,
        Guid? excludedId,
        CancellationToken cancellationToken);

    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    void Remove(Customer customer);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
