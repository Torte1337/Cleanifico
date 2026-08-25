using System.Security.Claims;
using System.Text.Encodings.Web;
using Cleanifico.Api;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.Security;
using Cleanifico.Application.TimeTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Domain.TimeTypes;
using Cleanifico.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Api.Tests;

internal sealed class ApiTestHost : IAsyncDisposable
{
    private readonly WebApplication application;

    private ApiTestHost(
        WebApplication application,
        HttpClient client,
        FakeCleaningTypeRepository repository,
        FakeTimeTypeRepository timeTypeRepository)
    {
        this.application = application;
        Client = client;
        Repository = repository;
        TimeTypeRepository = timeTypeRepository;
    }

    public HttpClient Client { get; }

    public FakeCleaningTypeRepository Repository { get; }

    public FakeTimeTypeRepository TimeTypeRepository { get; }

    public static async Task<ApiTestHost> StartAsync(params CleaningType[] seed)
        => await StartCoreAsync(SecurityRoles.Owner, false, seed, []);

    public static Task<ApiTestHost> StartAnonymousAsync(params CleaningType[] seed) =>
        StartCoreAsync(null, true, seed, []);

    public static Task<ApiTestHost> StartAsRoleAsync(string role, params CleaningType[] seed) =>
        StartCoreAsync(role, false, seed, []);

    public static Task<ApiTestHost> StartWithTimeTypesAsync(params TimeType[] seed) =>
        StartCoreAsync(SecurityRoles.Owner, false, [], seed);

    public static Task<ApiTestHost> StartAnonymousWithTimeTypesAsync(params TimeType[] seed) =>
        StartCoreAsync(null, true, [], seed);

    public static Task<ApiTestHost> StartAsRoleWithTimeTypesAsync(string role, params TimeType[] seed) =>
        StartCoreAsync(role, false, [], seed);

    private static async Task<ApiTestHost> StartCoreAsync(
        string? role,
        bool anonymous,
        CleaningType[] seed,
        TimeType[] timeTypeSeed)
    {
        var repository = new FakeCleaningTypeRepository(seed);
        var timeTypeRepository = new FakeTimeTypeRepository(timeTypeSeed);
        var userService = new FakeUserAdministrationService(role);
        var application = ApiApplication.Build(
            [
                "--environment", "Testing",
                "--ConnectionStrings:Cleanifico",
                "Server=localhost;Database=cleanifico_api_test;User=test;Password=not-used",
                "--SecurityBootstrap:Enabled", "false",
                "--TimeTypeBootstrap:Enabled", "false",
                "--Authentication:DataProtectionKeysPath", Path.Combine(Path.GetTempPath(), "cleanifico-api-tests-keys")
            ],
            services =>
            {
                services.AddSingleton<ICleaningTypeRepository>(repository);
                services.AddSingleton<ITimeTypeRepository>(timeTypeRepository);
                services.AddSingleton<IUserAdministrationService>(userService);
                services.AddSingleton(new TestIdentityOptions(role, anonymous));
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        _ => { });

                var activeHandler = services.Single(descriptor =>
                    descriptor.ServiceType == typeof(IAuthorizationHandler)
                    && descriptor.ImplementationType == typeof(ActiveUserAuthorizationHandler));
                services.Remove(activeHandler);
                services.AddSingleton<IAuthorizationHandler, TestActiveUserAuthorizationHandler>();
            });

        application.Urls.Add("http://127.0.0.1:0");
        await application.StartAsync();

        var server = application.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var address = Assert.Single(addresses ?? []);
        var client = new HttpClient { BaseAddress = new Uri(address) };

        return new ApiTestHost(application, client, repository, timeTypeRepository);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.StopAsync();
        await application.DisposeAsync();
    }
}

internal sealed record TestIdentityOptions(string? Role, bool Anonymous);

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TestIdentityOptions identityOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Cleanifico.Tests";
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (identityOptions.Anonymous)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId.ToString()),
            new(ClaimTypes.Name, "security-test@cleanifico.test"),
            new("cleanifico:active", "true")
        };

        if (identityOptions.Role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, identityOptions.Role));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}

internal sealed class TestActiveUserAuthorizationHandler
    : AuthorizationHandler<ActiveUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim("cleanifico:active", "true"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeUserAdministrationService(string? currentRole) : IUserAdministrationService
{
    private readonly List<UserAccount> users =
    [
        new(
            TestAuthenticationHandler.UserId,
            "Security",
            "Test",
            "security-test@cleanifico.test",
            true,
            currentRole is null ? [] : [currentRole],
            new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc))
    ];

    public Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserAccount>>(users);

    public Task<UserAccount> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(users.Single(user => user.Id == id));

    public Task<UserAccount> CreateAsync(CreateUserInput input, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<UserAccount> UpdateAsync(Guid id, UpdateUserInput input, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<string>> GetRolesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(SecurityRoles.All);

    public Task<UserAccount> UpdateRolesAsync(
        Guid id,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(users.Single(user => user.Id == id) with { Roles = [.. roles] });
}

internal sealed class FakeCleaningTypeRepository(params CleaningType[] seed) : ICleaningTypeRepository
{
    public List<CleaningType> Items { get; } = [.. seed];

    public Task<IReadOnlyList<CleaningType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = Items.AsEnumerable();

        if (search is not null)
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (isActive.HasValue)
        {
            query = query.Where(item => item.IsActive == isActive.Value);
        }

        return Task.FromResult<IReadOnlyList<CleaningType>>(
            [.. query.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)]);
    }

    public Task<CleaningType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(item => item.Id == id));

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item =>
            item.Id != excludedId
            && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item =>
            item.Id != excludedId
            && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(CleaningType cleaningType, CancellationToken cancellationToken)
    {
        Items.Add(cleaningType);
        return Task.CompletedTask;
    }

    public void Remove(CleaningType cleaningType) => Items.Remove(cleaningType);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeTimeTypeRepository(params TimeType[] seed) : ITimeTypeRepository
{
    private bool initialized;

    public List<TimeType> Items { get; } = [.. seed];

    public Task<IReadOnlyList<TimeType>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = Items.AsEnumerable();
        if (search is not null)
        {
            query = query.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (isActive.HasValue)
        {
            query = query.Where(item => item.IsActive == isActive.Value);
        }

        return Task.FromResult<IReadOnlyList<TimeType>>(
            [.. query.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)]);
    }

    public Task<TimeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(item => item.Id == id));

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item =>
            item.Id != excludedId
            && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item =>
            item.Id != excludedId
            && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(TimeType timeType, CancellationToken cancellationToken)
    {
        Items.Add(timeType);
        return Task.CompletedTask;
    }

    public void Remove(TimeType timeType) => Items.Remove(timeType);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task InitializeDefaultsAsync(
        IReadOnlyCollection<TimeType> defaults,
        DateTime initializedAtUtc,
        CancellationToken cancellationToken)
    {
        if (!initialized)
        {
            Items.AddRange(defaults);
            initialized = true;
        }

        return Task.CompletedTask;
    }
}
