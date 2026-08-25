using Cleanifico.Domain.CleaningTypes;

namespace Cleanifico.Application.CleaningTypes;

public sealed class CleaningTypeService(
    ICleaningTypeRepository repository,
    TimeProvider timeProvider) : ICleaningTypeService
{
    public Task<IReadOnlyList<CleaningType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        return repository.GetAllAsync(normalizedSearch, isActive, cancellationToken);
    }

    public async Task<CleaningType> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new CleaningTypeNotFoundException(id);

    public async Task<CleaningType> CreateAsync(
        CleaningTypeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var cleaningType = CleaningType.Create(
            Guid.NewGuid(),
            input.Name ?? string.Empty,
            input.Code ?? string.Empty,
            input.Description,
            input.SortOrder,
            UtcNow());

        await EnsureUniqueAsync(cleaningType.Name, cleaningType.Code, null, cancellationToken);
        await repository.AddAsync(cleaningType, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return cleaningType;
    }

    public async Task<CleaningType> UpdateAsync(
        Guid id,
        CleaningTypeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var cleaningType = await GetByIdAsync(id, cancellationToken);
        var normalizedName = CleaningType.NormalizeName(input.Name);
        var normalizedCode = CleaningType.NormalizeCode(input.Code);

        CleaningType.NormalizeDescription(input.Description);
        CleaningType.ValidateSortOrder(input.SortOrder);

        await EnsureUniqueAsync(normalizedName, normalizedCode, id, cancellationToken);

        cleaningType.Update(
            normalizedName,
            normalizedCode,
            input.Description,
            input.SortOrder,
            UtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return cleaningType;
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningType = await GetByIdAsync(id, cancellationToken);
        cleaningType.Activate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningType = await GetByIdAsync(id, cancellationToken);
        cleaningType.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningType = await GetByIdAsync(id, cancellationToken);
        repository.Remove(cleaningType);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(
        string name,
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(name, excludedId, cancellationToken))
        {
            throw new CleaningTypeConflictException(
                "name",
                "Ein Reinigungstyp mit diesem Namen ist bereits vorhanden.");
        }

        if (await repository.CodeExistsAsync(code, excludedId, cancellationToken))
        {
            throw new CleaningTypeConflictException(
                "code",
                "Ein Reinigungstyp mit diesem Kürzel ist bereits vorhanden.");
        }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
