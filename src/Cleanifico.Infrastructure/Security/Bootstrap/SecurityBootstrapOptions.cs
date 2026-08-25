namespace Cleanifico.Infrastructure.Security.Bootstrap;

public sealed class SecurityBootstrapOptions
{
    public const string SectionName = "SecurityBootstrap";

    public bool Enabled { get; set; } = true;

    public OwnerBootstrapOptions Owner { get; set; } = new();
}

public sealed class OwnerBootstrapOptions
{
    public bool Enabled { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? InitialPassword { get; set; }
}
