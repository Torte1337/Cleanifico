using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace Cleanifico.Web.Authentication;

public interface IOfficeApiRequestAuthenticator
{
    Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}

public sealed class OfficeApiRequestAuthenticator(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationStateProvider,
    IOptionsMonitor<CookieAuthenticationOptions> cookieOptions,
    TimeProvider timeProvider) : IOfficeApiRequestAuthenticator
{
    public async Task ApplyAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        if (request.Headers.Contains("Cookie"))
        {
            return;
        }

        var browserCookie = httpContextAccessor.HttpContext?
            .Request.Cookies[OfficeAuthentication.CookieName];
        if (!string.IsNullOrWhiteSpace(browserCookie))
        {
            AddCookie(request, browserCookie);
            return;
        }

        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var now = timeProvider.GetUtcNow();
        var properties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IsPersistent = false,
            IssuedUtc = now,
            ExpiresUtc = now.AddMinutes(5)
        };
        var ticket = new AuthenticationTicket(
            authenticationState.User,
            properties,
            OfficeAuthentication.CookieScheme);
        var protectedTicket = cookieOptions
            .Get(OfficeAuthentication.CookieScheme)
            .TicketDataFormat
            .Protect(ticket);

        AddCookie(request, protectedTicket);
    }

    private static void AddCookie(HttpRequestMessage request, string value) =>
        request.Headers.TryAddWithoutValidation(
            "Cookie",
            $"{OfficeAuthentication.CookieName}={value}");
}
