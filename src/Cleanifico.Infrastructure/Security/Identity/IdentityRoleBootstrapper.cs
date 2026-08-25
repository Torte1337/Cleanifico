using Cleanifico.Application.Security;
using Cleanifico.Contracts.Security;
using Microsoft.AspNetCore.Identity;

namespace Cleanifico.Infrastructure.Security.Identity;

public sealed class IdentityRoleBootstrapper(RoleManager<IdentityRole<Guid>> roleManager)
    : IRoleBootstrapper
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in SecurityRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"The role '{roleName}' could not be initialized: "
                    + string.Join(" ", result.Errors.Select(error => error.Description)));
            }
        }
    }
}
