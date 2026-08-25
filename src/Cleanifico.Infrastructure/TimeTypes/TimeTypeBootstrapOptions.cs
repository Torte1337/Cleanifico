namespace Cleanifico.Infrastructure.TimeTypes;

public sealed class TimeTypeBootstrapOptions
{
    public const string SectionName = "TimeTypeBootstrap";

    public bool Enabled { get; set; } = true;
}
