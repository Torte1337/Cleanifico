using System.Net.Http.Json;
using Cleanifico.Contracts.Authentication;
using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Cleanifico.Web.Authentication;

public static class OfficeAuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapOfficeAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", LoginAsync).AllowAnonymous();
        endpoints.MapPost("/auth/logout", LogoutAsync).RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        [FromForm] OfficeLoginForm form,
        IHttpClientFactory httpClientFactory,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClientFactory.CreateClient("OfficeApi").PostAsJsonAsync(
                "api/auth/login",
                new LoginRequest
                {
                    Email = form.Email,
                    Password = form.Password,
                    RememberMe = form.RememberMe
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Results.Redirect($"/login?error=invalid&returnUrl={Uri.EscapeDataString(SafeReturnUrl(form.ReturnUrl))}");
            }

            CopySetCookieHeaders(response, context.Response);
            return Results.Redirect(SafeReturnUrl(form.ReturnUrl));
        }
        catch (HttpRequestException exception)
        {
            loggerFactory.CreateLogger(typeof(OfficeAuthenticationEndpoints).FullName!)
                .LogWarning(exception, "The Cleanifico API could not be reached during sign-in.");
            return Results.Redirect($"/login?error=unavailable&returnUrl={Uri.EscapeDataString(SafeReturnUrl(form.ReturnUrl))}");
        }
    }

    private static async Task<IResult> LogoutAsync(
        IHttpClientFactory httpClientFactory,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClientFactory.CreateClient("OfficeApi")
                .PostAsync("api/auth/logout", null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                CopySetCookieHeaders(response, context.Response);
            }
        }
        catch (HttpRequestException exception)
        {
            loggerFactory.CreateLogger(typeof(OfficeAuthenticationEndpoints).FullName!)
                .LogWarning(exception, "The Cleanifico API could not be reached during sign-out.");
        }

        await context.SignOutAsync(OfficeAuthentication.CookieScheme);
        return Results.Redirect("/login");
    }

    private static void CopySetCookieHeaders(HttpResponseMessage source, HttpResponse target)
    {
        if (source.Headers.TryGetValues("Set-Cookie", out var values))
        {
            target.Headers.Append("Set-Cookie", new StringValues([.. values]));
        }
    }

    private static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)
            || !returnUrl.StartsWith("/", StringComparison.Ordinal)
            || returnUrl.StartsWith("//", StringComparison.Ordinal)
            || returnUrl.Contains('\\'))
        {
            return "/";
        }

        return returnUrl;
    }
}

public sealed class OfficeLoginForm
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
