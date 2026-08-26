using Cleanifico.Application.Licensing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Cleanifico.Infrastructure.Security.Authorization;

public sealed class LicensedProductAuthorizationHandler(ILicenseService licenseService)
    : AuthorizationHandler<LicensedProductRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LicensedProductRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await licenseService.CheckAsync();
        if (context.Resource is HttpContext httpContext)
        {
            httpContext.Items[LicenseAuthorizationContext.ResultItemKey] = result;
        }

        if (result.IsValid)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Cleanifico license status: {result.Status}."));
        }
    }
}
