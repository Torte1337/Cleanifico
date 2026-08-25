namespace Cleanifico.Application.CleaningTypes;

public sealed class CleaningTypeNotFoundException(Guid id)
    : Exception($"Der Reinigungstyp mit der ID '{id}' wurde nicht gefunden.")
{
    public Guid Id { get; } = id;
}
