namespace Cleanifico.Application.TimeTypes;

public sealed class TimeTypeNotFoundException(Guid id)
    : Exception($"Der Zeittyp mit der ID '{id}' wurde nicht gefunden.");
