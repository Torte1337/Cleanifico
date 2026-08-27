using Cleanifico.Application.EmployeeContracts;
using Cleanifico.Domain.EmployeeContracts;
using Cleanifico.Domain.Employees;

namespace Cleanifico.Application.Tests;

public sealed class EmployeeContractServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateUpdateFilterLifecycleAndDelete_Work()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        var repository = new FakeRepository([employee]);
        var service = CreateService(repository);

        EmployeeContractRecord created = await service.CreateAsync(Input(employee.Id, " V-100 "));
        EmployeeContractRecord updated = await service.UpdateAsync(
            created.Contract.Id,
            Input(employee.Id, "V-200", weeklyHours: 30));
        IReadOnlyList<EmployeeContractRecord> filtered = await service.GetAllAsync("Muster", true, employee.Id);
        await service.DeactivateAsync(created.Contract.Id);
        await service.ActivateAsync(created.Contract.Id);
        await service.DeleteAsync(created.Contract.Id);

        Assert.Equal("V-200", updated.Contract.ContractNumber);
        Assert.Equal(30, updated.Contract.WeeklyHours);
        Assert.Single(filtered);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task Create_RejectsUnknownEmployeeAndDuplicateNumber()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        EmployeeContract existing = CreateContract(employee.Id, "V-100", new(2025, 1, 1), new(2025, 12, 31));
        var service = CreateService(new FakeRepository([employee], existing));

        await Assert.ThrowsAsync<Cleanifico.Domain.Common.DomainValidationException>(() =>
            service.CreateAsync(Input(Guid.NewGuid(), "V-200")));
        EmployeeContractConflictException duplicate = await Assert.ThrowsAsync<EmployeeContractConflictException>(() =>
            service.CreateAsync(Input(employee.Id, " v-100 ", startDate: new(2026, 1, 1))));

        Assert.Equal("contractNumber", duplicate.Field);
    }

    [Fact]
    public async Task ActiveOverlaps_AreRejectedWhileSequentialHistoryIsPreserved()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        EmployeeContract first = CreateContract(employee.Id, "V-100", new(2025, 1, 1), new(2025, 12, 31));
        var repository = new FakeRepository([employee], first);
        var service = CreateService(repository);

        EmployeeContractConflictException overlap = await Assert.ThrowsAsync<EmployeeContractConflictException>(() =>
            service.CreateAsync(Input(
                employee.Id,
                "V-200",
                startDate: new(2025, 12, 1),
                endDate: new(2026, 11, 30),
                isPermanent: false)));
        EmployeeContractRecord next = await service.CreateAsync(Input(
            employee.Id,
            "V-201",
            startDate: new(2026, 1, 1)));

        Assert.Equal("startDate", overlap.Field);
        Assert.Equal(2, repository.Items.Count);
        Assert.Contains(first, repository.Items);
        Assert.Contains(next.Contract, repository.Items);
    }

    [Fact]
    public async Task Reactivate_RejectsOverlapWithAnotherActiveContract()
    {
        Employee employee = CreateEmployee("P-100", "Erika", "Muster");
        EmployeeContract oldContract = CreateContract(employee.Id, "V-100", new(2025, 1, 1), new(2025, 12, 31));
        oldContract.Deactivate(Now.UtcDateTime.AddDays(-1));
        EmployeeContract current = CreateContract(employee.Id, "V-200", new(2025, 6, 1), null);
        var service = CreateService(new FakeRepository([employee], oldContract, current));

        EmployeeContractConflictException exception = await Assert.ThrowsAsync<EmployeeContractConflictException>(() =>
            service.ActivateAsync(oldContract.Id));

        Assert.Equal("startDate", exception.Field);
        Assert.False(oldContract.IsActive);
    }

    private static EmployeeContractService CreateService(FakeRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static EmployeeContractInput Input(
        Guid employeeId,
        string contractNumber,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isPermanent = true,
        decimal weeklyHours = 40) =>
        new(
            contractNumber,
            employeeId,
            startDate ?? new DateOnly(2026, 1, 1),
            endDate,
            isPermanent,
            "Vollzeit",
            weeklyHours,
            173,
            30,
            null,
            null);

    private static Employee CreateEmployee(string number, string firstName, string lastName) =>
        Employee.Create(
            Guid.NewGuid(),
            new EmployeeData(number, firstName, lastName, null, null, null, null, null, null, null, null, null),
            Now.UtcDateTime.AddDays(-2));

    private static EmployeeContract CreateContract(
        Guid employeeId,
        string number,
        DateOnly startDate,
        DateOnly? endDate) =>
        EmployeeContract.Create(
            Guid.NewGuid(),
            new EmployeeContractData(
                number,
                employeeId,
                startDate,
                endDate,
                !endDate.HasValue,
                "Vollzeit",
                40,
                173,
                30,
                null,
                null),
            Now.UtcDateTime.AddDays(-1));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeRepository(
        IReadOnlyCollection<Employee> employees,
        params EmployeeContract[] seed) : IEmployeeContractRepository
    {
        public List<EmployeeContract> Items { get; } = [.. seed];

        public Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
            string? search,
            bool? isActive,
            Guid? employeeId,
            CancellationToken cancellationToken)
        {
            IEnumerable<EmployeeContractRecord> query = Records();
            if (search is not null) query = query.Where(record => record.Contract.ContractNumber.Contains(search, StringComparison.OrdinalIgnoreCase) || record.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (isActive.HasValue) query = query.Where(record => record.Contract.IsActive == isActive.Value);
            if (employeeId.HasValue) query = query.Where(record => record.Contract.EmployeeId == employeeId.Value);
            return Task.FromResult<IReadOnlyList<EmployeeContractRecord>>([.. query]);
        }

        public Task<EmployeeContractRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Records().SingleOrDefault(record => record.Contract.Id == id));
        public Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(contract => contract.Id == id));
        public Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken) =>
            Task.FromResult(employees.Any(employee => employee.Id == employeeId));
        public Task<bool> ContractNumberExistsAsync(string contractNumber, Guid? excludedId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(contract => contract.Id != excludedId && string.Equals(contract.ContractNumber, contractNumber, StringComparison.OrdinalIgnoreCase)));
        public Task<bool> HasOverlappingActiveContractAsync(Guid employeeId, DateOnly startDate, DateOnly? endDate, Guid? excludedId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(contract => contract.EmployeeId == employeeId && contract.IsActive && contract.Id != excludedId && (!endDate.HasValue || contract.StartDate <= endDate.Value) && (!contract.EndDate.HasValue || startDate <= contract.EndDate.Value)));
        public Task AddAsync(EmployeeContract contract, CancellationToken cancellationToken) { Items.Add(contract); return Task.CompletedTask; }
        public void Remove(EmployeeContract contract) => Items.Remove(contract);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private IEnumerable<EmployeeContractRecord> Records() => Items.Select(contract =>
        {
            Employee employee = employees.Single(item => item.Id == contract.EmployeeId);
            return new EmployeeContractRecord(contract, employee.EmployeeNumber, $"{employee.FirstName} {employee.LastName}");
        });
    }
}
