using Cleanifico.Application.Licensing;

namespace Cleanifico.Infrastructure.Security.Authorization;

public static class LicenseAuthorizationContext
{
    public static readonly object ResultItemKey = new();

    public static string UserMessage(LicenseStatus status) => status switch
    {
        LicenseStatus.Inactive => "Die Cleanifico-Lizenz ist nicht aktiv.",
        LicenseStatus.NotFound => "Für diese Cleanifico-Instanz wurde keine Lizenz gefunden.",
        LicenseStatus.Unavailable => "Die Lizenzprüfung ist derzeit nicht möglich.",
        _ => "Die Cleanifico-Lizenz ist aktiv."
    };
}
