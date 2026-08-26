using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Common;

namespace Cleanifico.Application.CleaningObjects;

public sealed class CleaningObjectService(ICleaningObjectRepository repository, TimeProvider timeProvider)
    : ICleaningObjectService
{
    public Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? customerId,
        CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(string.IsNullOrWhiteSpace(search) ? null : search.Trim(), isActive, customerId, cancellationToken);

    public async Task<CleaningObjectRecord> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await repository.GetRecordByIdAsync(id, cancellationToken) ?? throw new CleaningObjectNotFoundException(id);

    public async Task<CleaningObjectRecord> CreateAsync(CleaningObjectInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await EnsureCustomerExistsAsync(input.CustomerId, cancellationToken);
        var cleaningObject = CleaningObject.Create(Guid.NewGuid(), ToData(input), UtcNow());
        await EnsureNumberUniqueAsync(cleaningObject.ObjectNumber, null, cancellationToken);
        await repository.AddAsync(cleaningObject, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(cleaningObject.Id, cancellationToken);
    }

    public async Task<CleaningObjectRecord> UpdateAsync(Guid id, CleaningObjectInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var cleaningObject = await GetEntityAsync(id, cancellationToken);
        await EnsureCustomerExistsAsync(input.CustomerId, cancellationToken);
        var normalizedNumber = CleaningObject.NormalizeObjectNumber(input.ObjectNumber);
        await EnsureNumberUniqueAsync(normalizedNumber, id, cancellationToken);
        cleaningObject.Update(ToData(input), UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningObject = await GetEntityAsync(id, cancellationToken);
        cleaningObject.Activate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningObject = await GetEntityAsync(id, cancellationToken);
        cleaningObject.Deactivate(UtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaningObject = await GetEntityAsync(id, cancellationToken);
        repository.Remove(cleaningObject);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<CleaningObject> GetEntityAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new CleaningObjectNotFoundException(id);

    private async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainValidationException("customerId", "Ein Kunde ist erforderlich.");
        }

        if (!await repository.CustomerExistsAsync(customerId, cancellationToken))
        {
            throw new DomainValidationException("customerId", "Der ausgewählte Kunde wurde nicht gefunden.");
        }
    }

    private async Task EnsureNumberUniqueAsync(string objectNumber, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await repository.ObjectNumberExistsAsync(objectNumber, excludedId, cancellationToken))
        {
            throw new CleaningObjectConflictException("objectNumber", "Ein Objekt mit dieser Objektnummer ist bereits vorhanden.");
        }
    }

    private static CleaningObjectData ToData(CleaningObjectInput input) => new(
        input.ObjectNumber, input.CustomerId, input.Name, input.Street, input.PostalCode, input.City,
        input.Country, input.ContactFirstName, input.ContactLastName, input.ContactEmail,
        input.ContactPhone, input.AccessNotes, input.CleaningNotes);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
