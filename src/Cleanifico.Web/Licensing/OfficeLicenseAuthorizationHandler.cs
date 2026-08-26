using Microsoft.AspNetCore.Authorization;

namespace Cleanifico.Web.Licensing;

public sealed class OfficeLicenseAuthorizationHandler(IOfficeLicenseService licenseService)
    : AuthorizationHandler<OfficeLicenseRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OfficeLicenseRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await licenseService.GetStatusAsync();
        if (result.IsValid)
        {
            context.Succeed(requirement);
        }
    }
}
