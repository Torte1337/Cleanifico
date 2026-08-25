using System.Security.Claims;
using Cleanifico.Application.Security;
using Cleanifico.Contracts.Authentication;
using Cleanifico.Contracts.Security;

namespace Cleanifico.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        group.MapGet("/session", GetSessionAsync)
            .RequireAuthorization(SecurityPolicies.ActiveUser);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IUserAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var outcome = await authenticationService.PasswordSignInAsync(
            request.Email,
            request.Password,
            request.RememberMe,
            cancellationToken);

        return outcome == SignInOutcome.Success
            ? Results.NoContent()
            : Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Anmeldung fehlgeschlagen",
                detail: "E-Mail oder Passwort ist ungültig.");
    }

    private static async Task<IResult> LogoutAsync(
        IUserAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        await authenticationService.SignOutAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetSessionAsync(
        ClaimsPrincipal principal,
        IUserAdministrationService userService,
        CancellationToken cancellationToken)
    {
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        return Results.Ok(new CurrentUserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles));
    }
}
