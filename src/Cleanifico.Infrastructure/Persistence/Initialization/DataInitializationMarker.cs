namespace Cleanifico.Infrastructure.Persistence.Initialization;

public sealed class DataInitializationMarker
{
    private DataInitializationMarker()
    {
        Key = string.Empty;
    }

    private DataInitializationMarker(string key, DateTime completedAtUtc)
    {
        Key = key;
        CompletedAtUtc = completedAtUtc;
    }

    public string Key { get; private set; }

    public DateTime CompletedAtUtc { get; private set; }

    public static DataInitializationMarker Create(string key, DateTime completedAtUtc) =>
        new(key, completedAtUtc);
}
