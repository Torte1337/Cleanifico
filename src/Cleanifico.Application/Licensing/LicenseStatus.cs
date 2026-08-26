namespace Cleanifico.Application.Licensing;

public enum LicenseStatus
{
    Active,
    Inactive,
    NotFound,
    Unavailable
}

public sealed record LicenseCheckResult(LicenseStatus Status)
{
    public bool IsValid => Status == LicenseStatus.Active;
}
