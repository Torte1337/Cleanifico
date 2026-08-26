namespace Cleanifico.Application.CleaningObjects;

public sealed class CleaningObjectNotFoundException(Guid id)
    : Exception($"Das Objekt mit der ID '{id}' wurde nicht gefunden.");
