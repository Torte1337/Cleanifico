using Cleanifico.Application.Licensing;

namespace Cleanifico.Infrastructure.Security.Authorization;

public static class LicenseAuthorizationContext
{
    public static readonly object ResultItemKey = new();

    public static string UserMessage(LicenseStatus status) => status switch
    {
        LicenseStatus.NotActivated => "Diese Cleanifico-Installation wurde noch nicht aktiviert.",
        LicenseStatus.Grace => "Die Cleanifico-Lizenz befindet sich im Offline-Toleranzzeitraum.",
        LicenseStatus.Expired => "Die Cleanifico-Lizenz ist abgelaufen.",
        LicenseStatus.Invalid => "Der lokale Cleanifico-Lizenzzustand ist ungültig.",
        _ => "Die Cleanifico-Lizenz ist gültig."
    };
}
