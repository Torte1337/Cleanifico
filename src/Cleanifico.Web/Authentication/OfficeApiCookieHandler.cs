using Cleanifico.Contracts.Security;

namespace Cleanifico.Web.Authentication;

public sealed class OfficeApiCookieHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var cookie = httpContextAccessor.HttpContext?.Request.Cookies[OfficeAuthentication.CookieName];
        if (!string.IsNullOrWhiteSpace(cookie) && !request.Headers.Contains("Cookie"))
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"{OfficeAuthentication.CookieName}={cookie}");
        }

        request.Headers.TryAddWithoutValidation("X-Cleanifico-Office", "1");
        return base.SendAsync(request, cancellationToken);
    }
}
