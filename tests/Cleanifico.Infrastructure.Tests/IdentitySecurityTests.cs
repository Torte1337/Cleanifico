using Cleanifico.Application.Security;
using Cleanifico.Contracts.Security;
using Cleanifico.Infrastructure.Persistence;
using Cleanifico.Infrastructure.Security.Bootstrap;
using Cleanifico.Infrastructure.Security.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Tests;

public sealed class IdentitySecurityTests
{
    internal const string ValidPasswordForEnvironment = "Clean!fico-Test-93";

    [Fact]
    public async Task CreateUser_WithValidData_PersistsSafeAccountAndRole()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();

        var user = await environment.UserService.CreateAsync(new CreateUserInput(
            "  Erika ",
            " Muster ",
            " erika@example.test ",
            ValidPasswordForEnvironment,
            [SecurityRoles.Dispatcher],
            true));

        Assert.Equal("Erika", user.FirstName);
        Assert.Equal("Muster", user.LastName);
        Assert.Equal("erika@example.test", user.Email);
        Assert.Equal([SecurityRoles.Dispatcher], user.Roles);
        Assert.True(user.IsActive);

        var persisted = await environment.UserManager.FindByEmailAsync("erika@example.test");
        Assert.NotNull(persisted);
        Assert.NotNull(persisted.PasswordHash);
        Assert.DoesNotContain(
            ValidPasswordForEnvironment,
            persisted.PasswordHash,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateUser_RejectsDuplicateEmail()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        await environment.CreateUserAsync("owner@example.test", SecurityRoles.Owner);

        var exception = await Assert.ThrowsAsync<UserConflictException>(() =>
            environment.UserService.CreateAsync(new CreateUserInput(
                "Andere",
                "Person",
                "OWNER@example.test",
                ValidPasswordForEnvironment,
                [SecurityRoles.Administrator],
                true)));

        Assert.Equal("email", exception.Field);
    }

    [Fact]
    public async Task ActiveUser_CanSignInWithValidPassword()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        await environment.CreateUserAsync("owner@example.test", SecurityRoles.Owner);

        var outcome = await environment.AuthenticationService.PasswordSignInAsync(
            "owner@example.test",
            ValidPasswordForEnvironment,
            false);

        Assert.Equal(SignInOutcome.Success, outcome);
    }

    [Fact]
    public async Task InactiveUser_CannotSignIn()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        var user = await environment.CreateUserAsync("employee@example.test", SecurityRoles.Employee);
        await environment.UserService.DeactivateAsync(user.Id);

        var outcome = await environment.AuthenticationService.PasswordSignInAsync(
            "employee@example.test",
            ValidPasswordForEnvironment,
            false);

        Assert.Equal(SignInOutcome.Inactive, outcome);
    }

    [Fact]
    public async Task RepeatedFailedSignIns_LockAccount()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        await environment.CreateUserAsync("employee@example.test", SecurityRoles.Employee);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await environment.AuthenticationService.PasswordSignInAsync(
                "employee@example.test",
                "Wrong!Password-93",
                false);
        }

        var outcome = await environment.AuthenticationService.PasswordSignInAsync(
            "employee@example.test",
            ValidPasswordForEnvironment,
            false);

        Assert.Equal(SignInOutcome.LockedOut, outcome);
    }

    [Fact]
    public async Task RoleBootstrap_IsIdempotent()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();

        await environment.RoleBootstrapper.InitializeAsync();
        await environment.RoleBootstrapper.InitializeAsync();

        Assert.Equal(SecurityRoles.All.Count, await environment.RoleManager.Roles.CountAsync());
        Assert.Equal(
            SecurityRoles.All.OrderBy(role => role),
            await environment.RoleManager.Roles
                .Select(role => role.Name!)
                .OrderBy(role => role)
                .ToArrayAsync());
    }

    [Fact]
    public async Task UpdateRoles_CanGrantAndRevokeRole()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        var user = await environment.CreateUserAsync("dispatcher@example.test", SecurityRoles.Employee);

        var withDispatcher = await environment.UserService.UpdateRolesAsync(
            user.Id,
            [SecurityRoles.Employee, SecurityRoles.Dispatcher]);
        Assert.Contains(SecurityRoles.Dispatcher, withDispatcher.Roles);

        var withoutDispatcher = await environment.UserService.UpdateRolesAsync(
            user.Id,
            [SecurityRoles.Employee]);
        Assert.DoesNotContain(SecurityRoles.Dispatcher, withoutDispatcher.Roles);
    }

    [Fact]
    public async Task LastActiveOwner_CannotBeDeactivated()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        var owner = await environment.CreateUserAsync("owner@example.test", SecurityRoles.Owner);

        var exception = await Assert.ThrowsAsync<UserConflictException>(() =>
            environment.UserService.DeactivateAsync(owner.Id));

        Assert.Equal("owner", exception.Field);
        Assert.True((await environment.UserService.GetByIdAsync(owner.Id)).IsActive);
    }

    [Fact]
    public async Task LastActiveOwner_CannotLoseOwnerRole()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        var owner = await environment.UserService.CreateAsync(new CreateUserInput(
            "Olivia",
            "Owner",
            "owner@example.test",
            ValidPasswordForEnvironment,
            [SecurityRoles.Owner, SecurityRoles.Administrator],
            true));

        var exception = await Assert.ThrowsAsync<UserConflictException>(() =>
            environment.UserService.UpdateRolesAsync(owner.Id, [SecurityRoles.Administrator]));

        Assert.Equal("owner", exception.Field);
        Assert.Contains(
            SecurityRoles.Owner,
            (await environment.UserService.GetByIdAsync(owner.Id)).Roles);
    }

    [Fact]
    public async Task OwnerBootstrap_IsExplicitAndIdempotent()
    {
        await using var environment = await IdentityTestEnvironment.CreateAsync();
        var options = Options.Create(new SecurityBootstrapOptions
        {
            Enabled = true,
            Owner = new OwnerBootstrapOptions
            {
                Enabled = true,
                Email = "bootstrap-owner@example.test",
                FirstName = "Initial",
                LastName = "Owner",
                InitialPassword = ValidPasswordForEnvironment
            }
        });
        var hostedService = new IdentityBootstrapHostedService(
            environment.Provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            environment.Provider.GetRequiredService<ILogger<IdentityBootstrapHostedService>>());

        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.StartAsync(CancellationToken.None);

        await using var verificationScope = environment.Provider.CreateAsyncScope();
        var verificationUserManager = verificationScope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var owners = await verificationUserManager.GetUsersInRoleAsync(SecurityRoles.Owner);
        Assert.Single(owners);
        Assert.Equal("bootstrap-owner@example.test", owners[0].Email);
    }
}

internal sealed class IdentityTestEnvironment : IAsyncDisposable
{
    private static readonly DateTimeOffset TestNow =
        new(2026, 8, 25, 16, 0, 0, TimeSpan.Zero);
    private readonly AsyncServiceScope scope;

    private IdentityTestEnvironment(ServiceProvider provider, AsyncServiceScope scope)
    {
        Provider = provider;
        this.scope = scope;
        UserService = scope.ServiceProvider.GetRequiredService<IUserAdministrationService>();
        AuthenticationService = scope.ServiceProvider.GetRequiredService<IUserAuthenticationService>();
        RoleBootstrapper = scope.ServiceProvider.GetRequiredService<IRoleBootstrapper>();
        UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    }

    public ServiceProvider Provider { get; }
    public IUserAdministrationService UserService { get; }
    public IUserAuthenticationService AuthenticationService { get; }
    public IRoleBootstrapper RoleBootstrapper { get; }
    public UserManager<ApplicationUser> UserManager { get; }
    public RoleManager<IdentityRole<Guid>> RoleManager { get; }

    public static async Task<IdentityTestEnvironment> CreateAsync()
    {
        var services = new ServiceCollection();
        var databaseName = $"cleanifico-identity-{Guid.NewGuid()}";
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(TestNow));
        services.AddSingleton<OwnerProtectionGate>();
        services.AddHttpContextAccessor();
        services.AddDbContext<CleanificoDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentitySecurityDefaults.Configure)
            .AddEntityFrameworkStores<CleanificoDbContext>()
            .AddErrorDescriber<GermanIdentityErrorDescriber>()
            .AddDefaultTokenProviders();
        services.AddScoped<IUserAdministrationService, IdentityUserAdministrationService>();
        services.AddScoped<IUserAuthenticationService, IdentityUserAuthenticationService>();
        services.AddScoped<IRoleBootstrapper, IdentityRoleBootstrapper>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        var environment = new IdentityTestEnvironment(provider, scope);
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        signInManager.Context = accessor.HttpContext;
        await environment.RoleBootstrapper.InitializeAsync();
        return environment;
    }

    public Task<UserAccount> CreateUserAsync(string email, string role) =>
        UserService.CreateAsync(new CreateUserInput(
            "Test",
            "Benutzer",
            email,
            IdentitySecurityTests.ValidPasswordForEnvironment,
            [role],
            true));

    public async ValueTask DisposeAsync()
    {
        await scope.DisposeAsync();
        await Provider.DisposeAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
