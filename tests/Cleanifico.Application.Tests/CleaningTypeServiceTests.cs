using Cleanifico.Application.CleaningTypes;
using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Application.Tests;

public sealed class CleaningTypeServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 25, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_PersistsNormalizedCleaningType()
    {
        var repository = new FakeCleaningTypeRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new(" Unterhaltsreinigung ", " ur ", null, 10));

        Assert.Equal("Unterhaltsreinigung", result.Name);
        Assert.Equal("UR", result.Code);
        Assert.Equal(Now.UtcDateTime, result.CreatedAtUtc);
        Assert.Single(repository.Items);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Create_RejectsDuplicateName()
    {
        var repository = new FakeCleaningTypeRepository(CreateType(name: "Grundreinigung", code: "GR"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<CleaningTypeConflictException>(() =>
            service.CreateAsync(new(" grundreinigung ", "NEW", null, 0)));

        Assert.Equal("name", exception.Field);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Create_RejectsDuplicateCode()
    {
        var repository = new FakeCleaningTypeRepository(CreateType(name: "Grundreinigung", code: "GR"));
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<CleaningTypeConflictException>(() =>
            service.CreateAsync(new("Andere Reinigung", " gr ", null, 0)));

        Assert.Equal("code", exception.Field);
    }

    [Fact]
    public async Task Update_ExcludesCurrentRecordFromUniquenessCheck()
    {
        var existing = CreateType(name: "Grundreinigung", code: "GR");
        var repository = new FakeCleaningTypeRepository(existing);
        var service = CreateService(repository);

        var result = await service.UpdateAsync(
            existing.Id,
            new("Grundreinigung", "GR", "Intensiv", 5));

        Assert.Equal("Intensiv", result.Description);
        Assert.Equal(5, result.SortOrder);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task GetAll_TrimsSearchAndAppliesFilterAndDefaultSort()
    {
        var repository = new FakeCleaningTypeRepository(
            CreateType(name: "Unterhaltsreinigung", code: "UR", sortOrder: 20),
            CreateType(name: "Glasreinigung", code: "GL", sortOrder: 10),
            CreateType(name: "Inaktive Reinigung", code: "IR", sortOrder: 1, active: false));
        var service = CreateService(repository);

        var result = await service.GetAllAsync(" reinigung ", true);

        Assert.Equal(["Glasreinigung", "Unterhaltsreinigung"], result.Select(item => item.Name));
        Assert.Equal("reinigung", repository.LastSearch);
        Assert.True(repository.LastIsActive);
    }

    [Fact]
    public async Task GetById_ThrowsForMissingCleaningType()
    {
        var service = CreateService(new FakeCleaningTypeRepository());

        await Assert.ThrowsAsync<CleaningTypeNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeactivateAndActivate_PersistStatusChanges()
    {
        var existing = CreateType();
        var repository = new FakeCleaningTypeRepository(existing);
        var service = CreateService(repository);

        await service.DeactivateAsync(existing.Id);
        Assert.False(existing.IsActive);

        await service.ActivateAsync(existing.Id);
        Assert.True(existing.IsActive);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task Delete_RemovesExistingCleaningType()
    {
        var existing = CreateType();
        var repository = new FakeCleaningTypeRepository(existing);
        var service = CreateService(repository);

        await service.DeleteAsync(existing.Id);

        Assert.Empty(repository.Items);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    private static CleaningTypeService CreateService(FakeCleaningTypeRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static CleaningType CreateType(
        string name = "Unterhaltsreinigung",
        string code = "UR",
        int sortOrder = 10,
        bool active = true)
    {
        var cleaningType = CleaningType.Create(
            Guid.NewGuid(),
            name,
            code,
            null,
            sortOrder,
            Now.UtcDateTime.AddDays(-1));

        if (!active)
        {
            cleaningType.Deactivate(Now.UtcDateTime.AddHours(-1));
        }

        return cleaningType;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeCleaningTypeRepository(params CleaningType[] items) : ICleaningTypeRepository
    {
        public List<CleaningType> Items { get; } = [.. items];
        public int SaveChangesCalls { get; private set; }
        public string? LastSearch { get; private set; }
        public bool? LastIsActive { get; private set; }

        public Task<IReadOnlyList<CleaningType>> GetAllAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken = default)
        {
            LastSearch = search;
            LastIsActive = isActive;
            var query = Items.AsEnumerable();

            if (search is not null)
            {
                query = query.Where(item =>
                    item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (isActive.HasValue)
            {
                query = query.Where(item => item.IsActive == isActive.Value);
            }

            return Task.FromResult<IReadOnlyList<CleaningType>>(
                [.. query.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)]);
        }

        public Task<CleaningType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.Id == id));

        public Task<bool> NameExistsAsync(
            string name,
            Guid? excludedId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(item =>
                item.Id != excludedId
                && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> CodeExistsAsync(
            string code,
            Guid? excludedId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(item =>
                item.Id != excludedId
                && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(CleaningType cleaningType, CancellationToken cancellationToken = default)
        {
            Items.Add(cleaningType);
            return Task.CompletedTask;
        }

        public void Remove(CleaningType cleaningType) => Items.Remove(cleaningType);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
