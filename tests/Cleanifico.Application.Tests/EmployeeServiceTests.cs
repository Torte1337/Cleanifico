using Cleanifico.Application.Employees;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Application.Tests;

public sealed class EmployeeServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAndUpdate_PersistEmployee()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository);
        Employee created = await service.CreateAsync(Input(" P-100 ", " Erika ", " Muster "));
        Employee updated = await service.UpdateAsync(created.Id, Input("P-200", "Nina", "Neu", city: "Berlin"));

        Assert.Equal("P-200", updated.EmployeeNumber);
        Assert.Equal("Nina", updated.FirstName);
        Assert.Equal("Berlin", updated.City);
        Assert.Single(repository.Items);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task DuplicateEmployeeNumber_IsRejectedCaseInsensitively()
    {
        var repository = new FakeRepository(CreateEmployee("P-100", "Erika", "Muster"));
        var service = CreateService(repository);

        EmployeeConflictException exception = await Assert.ThrowsAsync<EmployeeConflictException>(() =>
            service.CreateAsync(Input(" p-100 ", "Paul", "Beispiel")));

        Assert.Equal("employeeNumber", exception.Field);
    }

    [Fact]
    public async Task SearchStatusLifecycleAndDelete_Work()
    {
        Employee active = CreateEmployee("P-100", "Erika", "Muster", "Berlin", "erika@example.test");
        Employee inactive = CreateEmployee("P-200", "Paul", "Beispiel", "Hamburg", phone: "+49 40 123");
        inactive.Deactivate(Now.UtcDateTime.AddHours(-1));
        var repository = new FakeRepository(active, inactive);
        var service = CreateService(repository);

        IReadOnlyList<Employee> byEmail = await service.GetAllAsync(" erika@example ", true);
        IReadOnlyList<Employee> byPhone = await service.GetAllAsync("40 123", false);
        await service.ActivateAsync(inactive.Id);
        await service.DeactivateAsync(active.Id);
        await service.DeleteAsync(active.Id);

        Assert.Equal([active], byEmail);
        Assert.Equal([inactive], byPhone);
        Assert.True(inactive.IsActive);
        Assert.DoesNotContain(active, repository.Items);
    }

    [Fact]
    public async Task Delete_RejectsEmployeeWithContractAndAllowsEmployeeWithoutContract()
    {
        Employee referenced = CreateEmployee("P-100", "Erika", "Muster");
        Employee unreferenced = CreateEmployee("P-200", "Paul", "Beispiel");
        var repository = new FakeRepository(referenced, unreferenced);
        repository.EmployeeIdsWithContracts.Add(referenced.Id);
        var service = CreateService(repository);

        EmployeeConflictException exception = await Assert.ThrowsAsync<EmployeeConflictException>(() =>
            service.DeleteAsync(referenced.Id));
        await service.DeleteAsync(unreferenced.Id);

        Assert.Equal("delete", exception.Field);
        Assert.Contains(referenced, repository.Items);
        Assert.DoesNotContain(unreferenced, repository.Items);
    }

    private static EmployeeService CreateService(FakeRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static EmployeeInput Input(
        string number,
        string firstName,
        string lastName,
        string? city = null) =>
        new(number, firstName, lastName, null, null, city, null, null, null, null, null, null);

    private static Employee CreateEmployee(
        string number,
        string firstName,
        string lastName,
        string? city = null,
        string? email = null,
        string? phone = null) =>
        Employee.Create(
            Guid.NewGuid(),
            new EmployeeData(number, firstName, lastName, null, null, city, null, email, phone, null, null, null),
            Now.UtcDateTime.AddDays(-1));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeRepository(params Employee[] seed) : IEmployeeRepository
    {
        public List<Employee> Items { get; } = [.. seed];
        public HashSet<Guid> EmployeeIdsWithContracts { get; } = [];
        public int SaveChangesCalls { get; private set; }

        public Task<IReadOnlyList<Employee>> GetAllAsync(string? search, bool? isActive, CancellationToken cancellationToken)
        {
            IEnumerable<Employee> query = Items;
            if (search is not null)
            {
                query = query.Where(employee =>
                    employee.EmployeeNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || employee.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || employee.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (employee.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (employee.Phone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (employee.MobilePhone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (employee.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (isActive.HasValue) query = query.Where(employee => employee.IsActive == isActive);
            return Task.FromResult<IReadOnlyList<Employee>>([.. query.OrderBy(employee => employee.LastName)]);
        }

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(employee => employee.Id == id));
        public Task<bool> EmployeeNumberExistsAsync(string number, Guid? excludedId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(employee => employee.Id != excludedId
                && string.Equals(employee.EmployeeNumber, number, StringComparison.OrdinalIgnoreCase)));
        public Task<bool> HasContractsAsync(Guid employeeId, CancellationToken cancellationToken) =>
            Task.FromResult(EmployeeIdsWithContracts.Contains(employeeId));
        public Task AddAsync(Employee employee, CancellationToken cancellationToken) { Items.Add(employee); return Task.CompletedTask; }
        public void Remove(Employee employee) => Items.Remove(employee);
        public Task SaveChangesAsync(CancellationToken cancellationToken) { SaveChangesCalls++; return Task.CompletedTask; }
    }
}
