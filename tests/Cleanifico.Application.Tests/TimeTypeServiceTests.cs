using Cleanifico.Application.TimeTypes;
using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Application.Tests;

public sealed class TimeTypeServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 25, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_PersistsNormalizedTimeType()
    {
        var repository = new FakeTimeTypeRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(Input(" Arbeitszeit ", " arb "));

        Assert.Equal("Arbeitszeit", result.Name);
        Assert.Equal("ARB", result.Code);
        Assert.Single(repository.Items);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Create_RejectsDuplicateName()
    {
        var repository = new FakeTimeTypeRepository(CreateType("Arbeitszeit", "ARB"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<TimeTypeConflictException>(() =>
            service.CreateAsync(Input(" arbeitszeit ", "NEU")));

        Assert.Equal("name", exception.Field);
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        var repository = new FakeTimeTypeRepository(CreateType("Arbeitszeit", "ARB"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<TimeTypeConflictException>(() =>
            service.CreateAsync(Input("Andere Zeit", " arb ")));

        Assert.Equal("code", exception.Field);
    }

    [Fact]
    public async Task Update_ChangesEveryConfigurableProperty()
    {
        var existing = CreateType("Arbeitszeit", "ARB");
        var repository = new FakeTimeTypeRepository(existing);
        var service = CreateService(repository);

        var result = await service.UpdateAsync(
            existing.Id,
            new("Urlaub", "URL", "Abwesenheit", true, true, false, true, "#805AD5", -5));

        Assert.Equal("Urlaub", result.Name);
        Assert.Equal("URL", result.Code);
        Assert.Equal("Abwesenheit", result.Description);
        Assert.True(result.CountsAsWorkTime);
        Assert.True(result.IsPaid);
        Assert.False(result.RequiresObject);
        Assert.True(result.IsAbsence);
        Assert.Equal("#805AD5", result.Color);
        Assert.Equal(-5, result.SortOrder);
    }

    [Fact]
    public async Task ActivateAndDeactivate_PersistStatusChanges()
    {
        var existing = CreateType("Arbeitszeit", "ARB");
        var repository = new FakeTimeTypeRepository(existing);
        var service = CreateService(repository);

        await service.DeactivateAsync(existing.Id);
        await service.ActivateAsync(existing.Id);

        Assert.True(existing.IsActive);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Initializer_IsIdempotentAndCreatesExpectedDefaults()
    {
        var repository = new FakeTimeTypeRepository();
        var initializer = new TimeTypeInitializer(repository, new FixedTimeProvider(Now));

        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        Assert.Equal(7, repository.Items.Count);
        Assert.Equal(
            ["ARB", "BES", "FAH", "KRK", "PAU", "SCH", "URL"],
            repository.Items.Select(item => item.Code).Order().ToArray());
        Assert.Equal(2, repository.InitializeCalls);
    }

    [Fact]
    public async Task ChangedDefault_IsNeverResetByInitializer()
    {
        var repository = new FakeTimeTypeRepository();
        var initializer = new TimeTypeInitializer(repository, new FixedTimeProvider(Now));
        await initializer.InitializeAsync();
        var service = CreateService(repository);
        var standard = repository.Items.Single(item => item.Code == "ARB");

        await service.UpdateAsync(
            standard.Id,
            new("Produktive Zeit", "PRD", null, false, false, false, true, null, 999));
        await initializer.InitializeAsync();

        var unchanged = Assert.Single(repository.Items, item => item.Id == standard.Id);
        Assert.Equal("Produktive Zeit", unchanged.Name);
        Assert.Equal("PRD", unchanged.Code);
        Assert.False(unchanged.CountsAsWorkTime);
        Assert.True(unchanged.IsAbsence);
        Assert.Equal(999, unchanged.SortOrder);
        Assert.DoesNotContain(repository.Items, item => item.Code == "ARB");
    }

    private static TimeTypeService CreateService(FakeTimeTypeRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static TimeTypeInput Input(string name, string code) =>
        new(name, code, null, true, true, true, false, "#2F855A", 10);

    private static TimeType CreateType(string name, string code) =>
        TimeType.Create(
            Guid.NewGuid(),
            name,
            code,
            null,
            true,
            true,
            true,
            false,
            "#2F855A",
            10,
            Now.UtcDateTime.AddDays(-1));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeTimeTypeRepository(params TimeType[] seed) : ITimeTypeRepository
    {
        private bool initialized;

        public List<TimeType> Items { get; } = [.. seed];
        public int SaveChangesCalls { get; private set; }
        public int InitializeCalls { get; private set; }

        public Task<IReadOnlyList<TimeType>> GetAllAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TimeType>>([.. Items]);

        public Task<TimeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item => item.Id == id));

        public Task<bool> NameExistsAsync(
            string name,
            Guid? excludedId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(item =>
                item.Id != excludedId
                && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> CodeExistsAsync(
            string code,
            Guid? excludedId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(item =>
                item.Id != excludedId
                && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(TimeType timeType, CancellationToken cancellationToken)
        {
            Items.Add(timeType);
            return Task.CompletedTask;
        }

        public void Remove(TimeType timeType) => Items.Remove(timeType);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }

        public Task InitializeDefaultsAsync(
            IReadOnlyCollection<TimeType> defaults,
            DateTime initializedAtUtc,
            CancellationToken cancellationToken)
        {
            InitializeCalls++;
            if (!initialized)
            {
                Items.AddRange(defaults);
                initialized = true;
            }

            return Task.CompletedTask;
        }
    }
}
