using Cleanifico.Domain.TimeTypes;

namespace Cleanifico.Application.TimeTypes;

public sealed class TimeTypeService(
    ITimeTypeRepository repository,
    TimeProvider timeProvider) : ITimeTypeService
{
    public Task<IReadOnlyList<TimeType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            isActive,
            cancellationToken);

    public async Task<TimeType> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new TimeTypeNotFoundException(id);

    public async Task<TimeType> CreateAsync(
        TimeTypeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var timeType = CreateTimeType(input, Guid.NewGuid(), UtcNow());
        await EnsureUniqueAsync(timeType.Name, timeType.Code, null, cancellationToken);
        await repository.AddAsync(timeType, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return timeType;
    }

    public async Task<TimeType> UpdateAsync(
        Guid id,
        TimeTypeInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var timeType = await GetByIdAsync(id, cancellationToken);
        var normalizedName = TimeType.NormalizeName(input.Name);
        var normalizedCode = TimeType.NormalizeCode(input.Code);
        TimeType.NormalizeDescription(input.Description);
        TimeType.NormalizeColor(input.Color);

        await EnsureUniqueAsync(normalizedName, normalizedCode, id, cancellationToken);
        timeType.Update(
            normalizedName,
            normalizedCode,
            input.Description,
            input.CountsAsWorkTime,
            input.IsPaid,
            input.RequiresObject,
            input.IsAbsence,
            input.Color,
            input.SortOrder,
            UtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return timeType;
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeType = await GetByIdAsync(id, cancellationToken);
        timeType.Activate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeType = await GetByIdAsync(id, cancellationToken);
        timeType.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeType = await GetByIdAsync(id, cancellationToken);
        repository.Remove(timeType);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static TimeType CreateTimeType(TimeTypeInput input, Guid id, DateTime createdAtUtc) =>
        TimeType.Create(
            id,
            input.Name ?? string.Empty,
            input.Code ?? string.Empty,
            input.Description,
            input.CountsAsWorkTime,
            input.IsPaid,
            input.RequiresObject,
            input.IsAbsence,
            input.Color,
            input.SortOrder,
            createdAtUtc);

    private async Task EnsureUniqueAsync(
        string name,
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(name, excludedId, cancellationToken))
        {
            throw new TimeTypeConflictException(
                "name",
                "Ein Zeittyp mit diesem Namen ist bereits vorhanden.");
        }

        if (await repository.CodeExistsAsync(code, excludedId, cancellationToken))
        {
            throw new TimeTypeConflictException(
                "code",
                "Ein Zeittyp mit diesem Kürzel ist bereits vorhanden.");
        }
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
