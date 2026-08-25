using Cleanifico.Application.Security;
using Cleanifico.Contracts.Security;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Security.Bootstrap;

public sealed class IdentityBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<SecurityBootstrapOptions> options,
    ILogger<IdentityBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var roleBootstrapper = scope.ServiceProvider.GetRequiredService<IRoleBootstrapper>();
        await roleBootstrapper.InitializeAsync(cancellationToken);

        var ownerOptions = options.Value.Owner;
        if (!ownerOptions.Enabled)
        {
            return;
        }

        ValidateOwnerOptions(ownerOptions);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var owners = await userManager.GetUsersInRoleAsync(SecurityRoles.Owner);
        if (owners.Count > 0)
        {
            logger.LogInformation("Owner bootstrap skipped because an owner already exists.");
            return;
        }

        if (await userManager.FindByEmailAsync(ownerOptions.Email!) is not null)
        {
            throw new InvalidOperationException(
                "Owner bootstrap cannot use an email address that belongs to an existing account.");
        }

        var userService = scope.ServiceProvider.GetRequiredService<IUserAdministrationService>();
        var owner = await userService.CreateAsync(
            new CreateUserInput(
                ownerOptions.FirstName,
                ownerOptions.LastName,
                ownerOptions.Email,
                ownerOptions.InitialPassword!,
                [SecurityRoles.Owner],
                true),
            cancellationToken);

        logger.LogInformation("Initial owner {UserId} was bootstrapped.", owner.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void ValidateOwnerOptions(OwnerBootstrapOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Email)
            || string.IsNullOrWhiteSpace(options.FirstName)
            || string.IsNullOrWhiteSpace(options.LastName)
            || string.IsNullOrWhiteSpace(options.InitialPassword))
        {
            throw new InvalidOperationException(
                "Explicit owner bootstrap requires email, first name, last name and initial password.");
        }
    }
}
