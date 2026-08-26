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
        endpoints.MapPost("/api/license/activate", ActivateAsync)
            .WithTags("Licensing")
            .RequireAuthorization(SecurityPolicies.ManageLicense);
        endpoints.MapPost("/api/license/refresh", RefreshAsync)
            .WithTags("Licensing")
            .RequireAuthorization(SecurityPolicies.ManageLicense);
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
            LicenseAuthorizationContext.UserMessage(result.Status),
            result.InstallationId,
            result.LicenseDisplayIdentifier,
            result.ValidUntilUtc,
            result.GraceUntilUtc,
            result.LastSuccessfulRefreshAtUtc,
            result.EffectiveFeatureCodes,
            result.DegradedReason));
    }

    private static async Task<IResult> ActivateAsync(
        ActivateLicenseRequest request,
        ILicenseActivationService activationService,
        CancellationToken cancellationToken)
    {
        LicenseOperationResult result = await activationService.ActivateAsync(
            request.LicenseKey ?? string.Empty,
            cancellationToken);
        return Results.Ok(ToResponse(result));
    }

    private static async Task<IResult> RefreshAsync(
        ILicenseRefreshService refreshService,
        CancellationToken cancellationToken)
    {
        LicenseOperationResult result = await refreshService.RefreshAsync(cancellationToken);
        return Results.Ok(ToResponse(result));
    }

    private static LicenseOperationResponse ToResponse(LicenseOperationResult result) => new(
        result.Status.ToString(),
        result.Succeeded,
        result.Status switch
        {
            LicenseOperationStatus.Success => "Die Lizenz wurde erfolgreich aktualisiert.",
            LicenseOperationStatus.InvalidLicenseKey => "Der Lizenzschlüssel besitzt kein gültiges Format.",
            LicenseOperationStatus.NotActivated => "Diese Installation wurde noch nicht aktiviert.",
            LicenseOperationStatus.LicensingUnavailable => "FergensHub ist derzeit nicht erreichbar; eine vorhandene lokale Lease bleibt bis zum Ende ihres Toleranzzeitraums wirksam.",
            LicenseOperationStatus.LicenseExpired => "Die Lizenz ist abgelaufen.",
            LicenseOperationStatus.LicenseSuspended => "Die Lizenz wurde ausgesetzt.",
            LicenseOperationStatus.LicenseRevoked => "Die Lizenz wurde widerrufen.",
            _ => "Die Lizenz konnte nicht aktualisiert werden."
        });
}
