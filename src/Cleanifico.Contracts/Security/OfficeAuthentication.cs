namespace Cleanifico.Contracts.Security;

public static class OfficeAuthentication
{
    public const string CookieScheme = "Identity.Application";
    public const string CookieName = ".Cleanifico.Office.Auth";
    public const string DataProtectionApplicationName = "Cleanifico.Office.SharedCookie";
    public const string DataProtectionKeysPathConfiguration = "Authentication:DataProtectionKeysPath";
}
