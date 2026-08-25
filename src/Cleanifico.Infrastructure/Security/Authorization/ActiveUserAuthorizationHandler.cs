using System.Security.Claims;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Cleanifico.Infrastructure.Security.Authorization;

public sealed class ActiveUserAuthorizationHandler(UserManager<ApplicationUser> userManager)
    : AuthorizationHandler<ActiveUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is { IsActive: true })
        {
            context.Succeed(requirement);
        }
    }
}
