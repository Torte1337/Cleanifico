using Cleanifico.Domain.Customers;

namespace Cleanifico.Application.Customers;

public sealed class CustomerService(
    ICustomerRepository repository,
    TimeProvider timeProvider) : ICustomerService
{
    public Task<IReadOnlyList<Customer>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            isActive,
            cancellationToken);

    public async Task<Customer> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new CustomerNotFoundException(id);

    public async Task<Customer> CreateAsync(
        CustomerInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var customer = Customer.Create(Guid.NewGuid(), ToData(input), UtcNow());
        await EnsureNumberUniqueAsync(customer.CustomerNumber, null, cancellationToken);
        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Customer> UpdateAsync(
        Guid id,
        CustomerInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var customer = await GetByIdAsync(id, cancellationToken);
        var normalizedNumber = Customer.NormalizeCustomerNumber(input.CustomerNumber);
        Customer.NormalizeCompanyName(input.CompanyName);
        await EnsureNumberUniqueAsync(normalizedNumber, id, cancellationToken);

        customer.Update(ToData(input), UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetByIdAsync(id, cancellationToken);
        customer.Activate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetByIdAsync(id, cancellationToken);
        customer.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetByIdAsync(id, cancellationToken);
        if (await repository.HasCleaningObjectsAsync(id, cancellationToken))
        {
            throw new CustomerConflictException(
                "delete",
                "Der Kunde besitzt mindestens ein Objekt und kann nicht endgültig gelöscht werden.");
        }

        repository.Remove(customer);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNumberUniqueAsync(
        string customerNumber,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.CustomerNumberExistsAsync(
                customerNumber,
                excludedId,
                cancellationToken))
        {
            throw new CustomerConflictException(
                "customerNumber",
                "Ein Kunde mit dieser Kundennummer ist bereits vorhanden.");
        }
    }

    private static CustomerData ToData(CustomerInput input) =>
        new(
            input.CustomerNumber,
            input.CompanyName,
            input.ContactFirstName,
            input.ContactLastName,
            input.Email,
            input.Phone,
            input.Street,
            input.PostalCode,
            input.City,
            input.Country,
            input.Notes);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
