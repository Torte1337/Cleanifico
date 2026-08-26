using Cleanifico.Application.CleaningObjects;
using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Common;

namespace Cleanifico.Application.Tests;

public sealed class CleaningObjectServiceTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_RequiresExistingCustomerAndReturnsCustomerData()
    {
        var repo = new FakeRepository(); var service = CreateService(repo);
        var result = await service.CreateAsync(Input(" OBJ-1 ", CustomerId, " Zentrale "));
        Assert.Equal("OBJ-1", result.CleaningObject.ObjectNumber);
        Assert.Equal("K-1", result.CustomerNumber);
        Assert.Single(repo.Items);
    }

    [Fact]
    public async Task Create_RejectsUnknownCustomer()
    {
        var repo = new FakeRepository { CustomerExists = false }; var service = CreateService(repo);
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            service.CreateAsync(Input("OBJ-1", CustomerId, "Zentrale")));
        Assert.Contains("customerId", ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateAndUpdate_RejectDuplicateNumber()
    {
        var existing = Item("OBJ-1", "Alt"); var repo = new FakeRepository(existing); var service = CreateService(repo);
        await Assert.ThrowsAsync<CleaningObjectConflictException>(() => service.CreateAsync(Input("obj-1", CustomerId, "Neu")));
        var second = Item("OBJ-2", "Zwei"); repo.Items.Add(second);
        await Assert.ThrowsAsync<CleaningObjectConflictException>(() => service.UpdateAsync(second.Id, Input("OBJ-1", CustomerId, "Zwei")));
    }

    [Fact]
    public async Task SearchAndFilters_AreForwardedNormalized()
    {
        var repo = new FakeRepository(Item("OBJ-1", "Zentrale")); var service = CreateService(repo);
        await service.GetAllAsync(" Kunde ", true, CustomerId);
        Assert.Equal("Kunde", repo.LastSearch); Assert.True(repo.LastActive); Assert.Equal(CustomerId, repo.LastCustomerId);
    }

    [Fact]
    public async Task UpdateAndLifecycle_PersistChanges()
    {
        var existing = Item("OBJ-1", "Alt"); var repo = new FakeRepository(existing); var service = CreateService(repo);
        var newCustomerId = Guid.NewGuid();
        var changed = await service.UpdateAsync(existing.Id, Input("OBJ-2", newCustomerId, "Neu"));
        await service.DeactivateAsync(existing.Id); Assert.False(existing.IsActive);
        await service.ActivateAsync(existing.Id); Assert.True(existing.IsActive);
        Assert.Equal("Neu", changed.CleaningObject.Name); Assert.Equal(newCustomerId, changed.CleaningObject.CustomerId);
        Assert.Equal(3, repo.SaveCalls);
    }

    [Fact]
    public async Task Delete_RemovesExistingObject()
    {
        var existing = Item("OBJ-1", "Alt"); var repo = new FakeRepository(existing); var service = CreateService(repo);
        await service.DeleteAsync(existing.Id); Assert.Empty(repo.Items);
    }

    [Fact]
    public async Task MissingObject_ProducesNotFound()
    {
        var service = CreateService(new FakeRepository());
        await Assert.ThrowsAsync<CleaningObjectNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    private static CleaningObjectService CreateService(FakeRepository repo) => new(repo, new FixedTimeProvider(Now));
    private static CleaningObjectInput Input(string number, Guid customerId, string name) =>
        new(number, customerId, name, null, null, null, null, null, null, null, null, null, null);
    private static CleaningObject Item(string number, string name) => CleaningObject.Create(
        Guid.NewGuid(), new CleaningObjectData(number, CustomerId, name, null, null, null, null, null, null, null, null, null, null), Now.UtcDateTime.AddDays(-1));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }
    private sealed class FakeRepository(params CleaningObject[] seed) : ICleaningObjectRepository
    {
        public List<CleaningObject> Items { get; } = [.. seed]; public bool CustomerExists { get; set; } = true;
        public int SaveCalls { get; private set; } public string? LastSearch { get; private set; }
        public bool? LastActive { get; private set; } public Guid? LastCustomerId { get; private set; }
        public Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(string? search, bool? active, Guid? customerId, CancellationToken ct)
        { LastSearch=search; LastActive=active; LastCustomerId=customerId; return Task.FromResult<IReadOnlyList<CleaningObjectRecord>>([.. Items.Select(Record)]); }
        public Task<CleaningObjectRecord?> GetRecordByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Items.Where(x=>x.Id==id).Select(Record).SingleOrDefault());
        public Task<CleaningObject?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Items.SingleOrDefault(x=>x.Id==id));
        public Task<bool> ObjectNumberExistsAsync(string number, Guid? excludedId, CancellationToken ct) => Task.FromResult(Items.Any(x=>x.Id!=excludedId && string.Equals(x.ObjectNumber, number, StringComparison.OrdinalIgnoreCase)));
        public Task<bool> CustomerExistsAsync(Guid id, CancellationToken ct) => Task.FromResult(CustomerExists && id != Guid.Empty);
        public Task AddAsync(CleaningObject item, CancellationToken ct) { Items.Add(item); return Task.CompletedTask; }
        public void Remove(CleaningObject item) => Items.Remove(item);
        public Task SaveChangesAsync(CancellationToken ct) { SaveCalls++; return Task.CompletedTask; }
        private static CleaningObjectRecord Record(CleaningObject item) => new(item, "K-1", "Kunde GmbH");
    }
}
