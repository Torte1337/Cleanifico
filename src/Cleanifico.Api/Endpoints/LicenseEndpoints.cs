using Cleanifico.Application.Licensing;
using Cleanifico.Contracts.Licensing;
using Cleanifico.Contracts.Security;
using Cleanifico.Infrastructure.Security.Authorization;

namespace Cleanifico.Api.Endpoints;

public static class LicenseEndpoints
{
    public static IEndpointRouteBuilder MapLicenseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/license/status", GetStatusAsync)
            .WithTags("Licensing")
            .RequireAuthorization(SecurityPolicies.ActiveUser);
        return endpoints;
    }

    private static async Task<IResult> GetStatusAsync(
        ILicenseService licenseService,
        CancellationToken cancellationToken)
    {
        var result = await licenseService.CheckAsync(cancellationToken);
        return Results.Ok(new LicenseStatusResponse(
            result.Status.ToString(),
            result.IsValid,
            LicenseAuthorizationContext.UserMessage(result.Status)));
    }
}
