using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Cleanifico.Contracts.Authentication;
using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Cleanifico.Web.Authentication;

public sealed class OfficeCookieEvents(
    IHttpClientFactory httpClientFactory,
    ILogger<OfficeCookieEvents> logger) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var cookie = context.Request.Cookies[OfficeAuthentication.CookieName];
        if (string.IsNullOrWhiteSpace(cookie))
        {
            await RejectAsync(context);
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/session");
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"{OfficeAuthentication.CookieName}={cookie}");
            request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");

            using var response = await httpClientFactory.CreateClient("OfficeApi").SendAsync(request);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                await RejectAsync(context);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Office session validation failed with status {StatusCode}.",
                    (int)response.StatusCode);
                await RejectAsync(context);
                return;
            }

            var currentUser = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
            var cookieRoles = context.Principal?.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray() ?? [];
            var currentRoles = currentUser?.Roles
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray() ?? [];

            if (currentUser is null || !cookieRoles.SequenceEqual(currentRoles, StringComparer.Ordinal))
            {
                await RejectAsync(context);
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Office session validation could not reach the API.");
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(OfficeAuthentication.CookieScheme);
    }
}
