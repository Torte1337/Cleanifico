namespace Cleanifico.Contracts.Security;

public static class SecurityRoles
{
    public const string Owner = "Owner";
    public const string Administrator = "Administrator";
    public const string Dispatcher = "Dispatcher";
    public const string ObjectManager = "ObjectManager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All =
    [
        Owner,
        Administrator,
        Dispatcher,
        ObjectManager,
        Employee
    ];

    public static readonly IReadOnlyList<string> Office =
    [
        Owner,
        Administrator,
        Dispatcher,
        ObjectManager
    ];

    public static readonly IReadOnlyList<string> Administrators = [Owner, Administrator];

    public static string GetDisplayName(string role) => role switch
    {
        Owner => "Inhaber",
        Administrator => "Administrator",
        Dispatcher => "Disposition",
        ObjectManager => "Objektleitung",
        Employee => "Mitarbeiter",
        _ => role
    };
}
