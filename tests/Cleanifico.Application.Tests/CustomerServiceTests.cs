using Cleanifico.Application.Customers;
using Cleanifico.Domain.Customers;

namespace Cleanifico.Application.Tests;

public sealed class CustomerServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 26, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_PersistsValidCustomer()
    {
        var repository = new FakeCustomerRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(Input(" K-100 ", " Muster GmbH "));

        Assert.Equal("K-100", result.CustomerNumber);
        Assert.Equal("Muster GmbH", result.CompanyName);
        Assert.Single(repository.Items);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Create_RejectsDuplicateCustomerNumber()
    {
        var repository = new FakeCustomerRepository(CreateCustomer("K-100", "Erste Firma"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<CustomerConflictException>(() =>
            service.CreateAsync(Input(" k-100 ", "Zweite Firma")));

        Assert.Equal("customerNumber", exception.Field);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Update_ChangesCustomerAndAllowsCurrentNumber()
    {
        var existing = CreateCustomer("K-100", "Alte Firma");
        var repository = new FakeCustomerRepository(existing);
        var service = CreateService(repository);

        var result = await service.UpdateAsync(
            existing.Id,
            new(
                "K-100",
                "Neue Firma",
                "Erika",
                "Muster",
                "erika@example.test",
                "+49 30 123",
                "Straße 1",
                "10115",
                "Berlin",
                "Deutschland",
                "Notiz"));

        Assert.Equal("Neue Firma", result.CompanyName);
        Assert.Equal("Erika", result.ContactFirstName);
        Assert.Equal("Berlin", result.City);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Search_UsesRequiredFieldsAndStatusFilter()
    {
        var active = CreateCustomer("K-100", "Nord GmbH", "Berlin", "Erika", "Muster");
        var inactive = CreateCustomer("K-200", "Süd GmbH", "München", "Paul", "Beispiel");
        inactive.Deactivate(Now.UtcDateTime.AddHours(-1));
        var repository = new FakeCustomerRepository(active, inactive);
        var service = CreateService(repository);

        var byCity = await service.GetAllAsync(" berlin ", true);
        var byContact = await service.GetAllAsync("Paul", false);

        Assert.Equal(["Nord GmbH"], byCity.Select(customer => customer.CompanyName));
        Assert.Equal(["Süd GmbH"], byContact.Select(customer => customer.CompanyName));
        Assert.Equal("Paul", repository.LastSearch);
        Assert.False(repository.LastIsActive);
    }

    [Fact]
    public async Task ActivateDeactivateAndDelete_PersistLifecycle()
    {
        var existing = CreateCustomer("K-100", "Muster GmbH");
        var repository = new FakeCustomerRepository(existing);
        var service = CreateService(repository);

        await service.DeactivateAsync(existing.Id);
        Assert.False(existing.IsActive);
        await service.ActivateAsync(existing.Id);
        Assert.True(existing.IsActive);
        await service.DeleteAsync(existing.Id);

        Assert.Empty(repository.Items);
        Assert.Equal(3, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Delete_RejectsCustomerWithCleaningObjects()
    {
        var existing = CreateCustomer("K-100", "Muster GmbH");
        var repository = new FakeCustomerRepository(existing) { HasCleaningObjects = true };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<CustomerConflictException>(() => service.DeleteAsync(existing.Id));

        Assert.Equal("delete", exception.Field);
        Assert.Contains("mindestens ein Objekt", exception.Message);
        Assert.Single(repository.Items);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    private static CustomerService CreateService(FakeCustomerRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static CustomerInput Input(string number, string company) =>
        new(number, company, null, null, null, null, null, null, null, null, null);

    private static Customer CreateCustomer(
        string number,
        string company,
        string? city = null,
        string? firstName = null,
        string? lastName = null) =>
        Customer.Create(
            Guid.NewGuid(),
            new CustomerData(
                number,
                company,
                firstName,
                lastName,
                null,
                null,
                null,
                null,
                city,
                null,
                null),
            Now.UtcDateTime.AddDays(-1));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeCustomerRepository(params Customer[] seed) : ICustomerRepository
    {
        public List<Customer> Items { get; } = [.. seed];
        public int SaveChangesCalls { get; private set; }
        public string? LastSearch { get; private set; }
        public bool? LastIsActive { get; private set; }
        public bool HasCleaningObjects { get; set; }

        public Task<IReadOnlyList<Customer>> GetAllAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken)
        {
            LastSearch = search;
            LastIsActive = isActive;
            var query = Items.AsEnumerable();
            if (search is not null)
            {
                query = query.Where(customer =>
                    customer.CustomerNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || customer.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (customer.ContactFirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (customer.ContactLastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (customer.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (isActive.HasValue)
            {
                query = query.Where(customer => customer.IsActive == isActive.Value);
            }

            return Task.FromResult<IReadOnlyList<Customer>>(
                [.. query.OrderBy(customer => customer.CompanyName)]);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(customer => customer.Id == id));

        public Task<bool> CustomerNumberExistsAsync(
            string customerNumber,
            Guid? excludedId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(customer =>
                customer.Id != excludedId
                && string.Equals(
                    customer.CustomerNumber,
                    customerNumber,
                    StringComparison.OrdinalIgnoreCase)));

        public Task<bool> HasCleaningObjectsAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(HasCleaningObjects);

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            Items.Add(customer);
            return Task.CompletedTask;
        }

        public void Remove(Customer customer) => Items.Remove(customer);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
