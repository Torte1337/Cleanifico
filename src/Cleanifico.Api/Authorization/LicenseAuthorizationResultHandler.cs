using Cleanifico.Application.Licensing;
using Cleanifico.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Cleanifico.Api.Authorization;

public sealed class LicenseAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden
            && context.Items[LicenseAuthorizationContext.ResultItemKey] is LicenseCheckResult result
            && !result.IsValid)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Cleanifico-Lizenz nicht gültig",
                detail: LicenseAuthorizationContext.UserMessage(result.Status),
                extensions: new Dictionary<string, object?>
                {
                    ["licenseStatus"] = result.Status.ToString()
                }).ExecuteAsync(context);
            return;
        }

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
