using System.ComponentModel.DataAnnotations;

namespace Cleanifico.Contracts.Licensing;

public sealed class ActivateLicenseRequest
{
    [Required]
    [StringLength(48, MinimumLength = 48)]
    public string? LicenseKey { get; init; }

    public override string ToString() =>
        $"{nameof(ActivateLicenseRequest)} {{ LicenseKey = [REDACTED] }}";
}
