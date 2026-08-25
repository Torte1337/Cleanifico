namespace Cleanifico.Domain.Common;

public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string field, string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [field] = [message]
        };
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
