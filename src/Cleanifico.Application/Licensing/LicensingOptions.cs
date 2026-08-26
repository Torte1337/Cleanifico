namespace Cleanifico.Application.Licensing;

public sealed class LicensingOptions
{
    public const string SectionName = "Licensing";
    public const string CleanificoProductCode = "CLEANIFICO";
    public const string DefaultStatePath = "/app/config/license-state.json";
    public static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromHours(24);
    public static readonly TimeSpan MinimumRefreshInterval = TimeSpan.FromHours(1);
    public static readonly TimeSpan MaximumRefreshInterval = TimeSpan.FromDays(30);

    public Uri? BaseUrl { get; init; }

    public string ProductCode { get; init; } = CleanificoProductCode;

    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan RefreshInterval { get; init; } = DefaultRefreshInterval;

    public string StatePath { get; init; } = DefaultStatePath;
}
