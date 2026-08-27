using System.Security.Claims;
using System.Text.Encodings.Web;
using Cleanifico.Api;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.CleaningObjects;
using Cleanifico.Application.Customers;
using Cleanifico.Application.Employees;
using Cleanifico.Application.EmployeeContracts;
using Cleanifico.Application.Licensing;
using Cleanifico.Application.Security;
using Cleanifico.Application.TimeTypes;
using Cleanifico.Contracts.Security;
using Cleanifico.Domain.CleaningTypes;
using Cleanifico.Domain.CleaningObjects;
using Cleanifico.Domain.Customers;
using Cleanifico.Domain.Employees;
using Cleanifico.Domain.EmployeeContracts;
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
        FakeCleaningObjectRepository cleaningObjectRepository,
        FakeTimeTypeRepository timeTypeRepository,
        FakeCustomerRepository customerRepository,
        FakeEmployeeRepository employeeRepository,
        FakeEmployeeContractRepository employeeContractRepository)
    {
        this.application = application;
        Client = client;
        Repository = repository;
        CleaningObjectRepository = cleaningObjectRepository;
        TimeTypeRepository = timeTypeRepository;
        CustomerRepository = customerRepository;
        EmployeeRepository = employeeRepository;
        EmployeeContractRepository = employeeContractRepository;
    }

    public HttpClient Client { get; }

    public FakeCleaningTypeRepository Repository { get; }

    public FakeCleaningObjectRepository CleaningObjectRepository { get; }

    public FakeTimeTypeRepository TimeTypeRepository { get; }

    public FakeCustomerRepository CustomerRepository { get; }

    public FakeEmployeeRepository EmployeeRepository { get; }

    public FakeEmployeeContractRepository EmployeeContractRepository { get; }

    public static async Task<ApiTestHost> StartAsync(params CleaningType[] seed)
        => await StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, seed, [], [], [], []);

    public static Task<ApiTestHost> StartAnonymousAsync(params CleaningType[] seed) =>
        StartCoreAsync(null, true, LicenseStatus.Valid, seed, [], [], [], []);

    public static Task<ApiTestHost> StartAsRoleAsync(string role, params CleaningType[] seed) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, seed, [], [], [], []);

    public static Task<ApiTestHost> StartWithTimeTypesAsync(params TimeType[] seed) =>
        StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, [], seed, [], [], []);

    public static Task<ApiTestHost> StartAnonymousWithTimeTypesAsync(params TimeType[] seed) =>
        StartCoreAsync(null, true, LicenseStatus.Valid, [], seed, [], [], []);

    public static Task<ApiTestHost> StartAsRoleWithTimeTypesAsync(string role, params TimeType[] seed) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, [], seed, [], [], []);

    public static Task<ApiTestHost> StartWithCustomersAsync(params Customer[] seed) =>
        StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, [], [], seed, [], []);

    public static Task<ApiTestHost> StartAnonymousWithCustomersAsync(params Customer[] seed) =>
        StartCoreAsync(null, true, LicenseStatus.Valid, [], [], seed, [], []);

    public static Task<ApiTestHost> StartAsRoleWithCustomersAsync(string role, params Customer[] seed) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, [], [], seed, [], []);

    public static Task<ApiTestHost> StartWithObjectsAsync(Customer[] customers, params CleaningObject[] seed) =>
        StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, [], [], customers, seed, []);

    public static Task<ApiTestHost> StartAnonymousWithObjectsAsync() =>
        StartCoreAsync(null, true, LicenseStatus.Valid, [], [], [], [], []);

    public static Task<ApiTestHost> StartAsRoleWithObjectsAsync(string role, Customer[] customers, params CleaningObject[] seed) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, [], [], customers, seed, []);

    public static Task<ApiTestHost> StartWithEmployeesAsync(params Employee[] seed) =>
        StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, [], [], [], [], seed);

    public static Task<ApiTestHost> StartAnonymousWithEmployeesAsync(params Employee[] seed) =>
        StartCoreAsync(null, true, LicenseStatus.Valid, [], [], [], [], seed);

    public static Task<ApiTestHost> StartAsRoleWithEmployeesAsync(string role, params Employee[] seed) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, [], [], [], [], seed);

    public static Task<ApiTestHost> StartEmployeesWithLicenseAsync(LicenseStatus status) =>
        StartCoreAsync(SecurityRoles.Owner, false, status, [], [], [], [], []);

    public static Task<ApiTestHost> StartWithEmployeeContractsAsync(
        Employee[] employees,
        params EmployeeContract[] contracts) =>
        StartCoreAsync(SecurityRoles.Owner, false, LicenseStatus.Valid, [], [], [], [], employees, contracts);

    public static Task<ApiTestHost> StartAnonymousWithEmployeeContractsAsync(
        Employee[] employees,
        params EmployeeContract[] contracts) =>
        StartCoreAsync(null, true, LicenseStatus.Valid, [], [], [], [], employees, contracts);

    public static Task<ApiTestHost> StartAsRoleWithEmployeeContractsAsync(
        string role,
        Employee[] employees,
        params EmployeeContract[] contracts) =>
        StartCoreAsync(role, false, LicenseStatus.Valid, [], [], [], [], employees, contracts);

    public static Task<ApiTestHost> StartEmployeeContractsWithLicenseAsync(LicenseStatus status) =>
        StartCoreAsync(SecurityRoles.Owner, false, status, [], [], [], [], [], []);

    public static Task<ApiTestHost> StartWithLicenseAsync(
        LicenseStatus licenseStatus,
        string? role = SecurityRoles.Owner,
        bool anonymous = false) =>
        StartCoreAsync(role, anonymous, licenseStatus, [], [], [], [], []);

    private static async Task<ApiTestHost> StartCoreAsync(
        string? role,
        bool anonymous,
        LicenseStatus licenseStatus,
        CleaningType[] seed,
        TimeType[] timeTypeSeed,
        Customer[] customerSeed,
        CleaningObject[] cleaningObjectSeed,
        Employee[] employeeSeed,
        EmployeeContract[]? employeeContractSeed = null)
    {
        var repository = new FakeCleaningTypeRepository(seed);
        var timeTypeRepository = new FakeTimeTypeRepository(timeTypeSeed);
        var customerRepository = new FakeCustomerRepository(customerSeed);
        var cleaningObjectRepository = new FakeCleaningObjectRepository(customerRepository, cleaningObjectSeed);
        var employeeRepository = new FakeEmployeeRepository(employeeSeed);
        var employeeContractRepository = new FakeEmployeeContractRepository(
            employeeRepository,
            employeeContractSeed ?? []);
        customerRepository.CleaningObjects = cleaningObjectRepository.Items;
        employeeRepository.Contracts = employeeContractRepository.Items;
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
                services.AddSingleton<ICleaningObjectRepository>(cleaningObjectRepository);
                services.AddSingleton<ICustomerRepository>(customerRepository);
                services.AddSingleton<IEmployeeRepository>(employeeRepository);
                services.AddSingleton<IEmployeeContractRepository>(employeeContractRepository);
                services.AddSingleton<ITimeTypeRepository>(timeTypeRepository);
                services.AddSingleton<IUserAdministrationService>(userService);
                services.AddSingleton<ILicenseService>(new FakeLicenseService(licenseStatus));
                services.AddSingleton<FakeLicenseOperationService>();
                services.AddSingleton<ILicenseActivationService>(provider =>
                    provider.GetRequiredService<FakeLicenseOperationService>());
                services.AddSingleton<ILicenseRefreshService>(provider =>
                    provider.GetRequiredService<FakeLicenseOperationService>());
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

        return new ApiTestHost(
            application,
            client,
            repository,
            cleaningObjectRepository,
            timeTypeRepository,
            customerRepository,
            employeeRepository,
            employeeContractRepository);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.StopAsync();
        await application.DisposeAsync();
    }
}

internal sealed class FakeLicenseService(LicenseStatus status) : ILicenseService
{
    public Task<LicenseCheckResult> CheckAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseCheckResult(status));
}

internal sealed class FakeLicenseOperationService :
    ILicenseActivationService,
    ILicenseRefreshService
{
    public Task<LicenseOperationResult> ActivateAsync(
        string licenseKey,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseOperationResult(LicenseOperationStatus.Success));

    public Task<LicenseOperationResult> RefreshAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new LicenseOperationResult(LicenseOperationStatus.Success));
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

internal sealed class FakeCustomerRepository(params Customer[] seed) : ICustomerRepository
{
    public List<Customer> Items { get; } = [.. seed];
    public IReadOnlyCollection<CleaningObject> CleaningObjects { get; set; } = [];

    public Task<IReadOnlyList<Customer>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = Items.AsEnumerable();
        if (search is not null)
        {
            query = query.Where(customer =>
                customer.CustomerNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || customer.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (customer.ContactFirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (customer.ContactLastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (customer.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (isActive.HasValue)
        {
            query = query.Where(customer => customer.IsActive == isActive.Value);
        }

        return Task.FromResult<IReadOnlyList<Customer>>(
            [.. query.OrderBy(customer => customer.CompanyName).ThenBy(customer => customer.CustomerNumber)]);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(customer => customer.Id == id));

    public Task<bool> CustomerNumberExistsAsync(
        string customerNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(customer =>
            customer.Id != excludedId
            && string.Equals(
                customer.CustomerNumber,
                customerNumber,
                StringComparison.OrdinalIgnoreCase)));

    public Task<bool> HasCleaningObjectsAsync(Guid customerId, CancellationToken cancellationToken) =>
        Task.FromResult(CleaningObjects.Any(item => item.CustomerId == customerId));

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        Items.Add(customer);
        return Task.CompletedTask;
    }

    public void Remove(Customer customer) => Items.Remove(customer);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeCleaningObjectRepository(
    FakeCustomerRepository customers,
    params CleaningObject[] seed) : ICleaningObjectRepository
{
    public List<CleaningObject> Items { get; } = [.. seed];

    public Task<IReadOnlyList<CleaningObjectRecord>> GetAllAsync(
        string? search, bool? isActive, Guid? customerId, CancellationToken cancellationToken)
    {
        var query = Records();
        if (search is not null)
        {
            query = query.Where(record =>
                record.CleaningObject.ObjectNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || record.CleaningObject.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (record.CleaningObject.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (record.CleaningObject.ContactFirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (record.CleaningObject.ContactLastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || record.CustomerCompanyName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        if (isActive.HasValue) query = query.Where(record => record.CleaningObject.IsActive == isActive.Value);
        if (customerId.HasValue) query = query.Where(record => record.CleaningObject.CustomerId == customerId.Value);
        return Task.FromResult<IReadOnlyList<CleaningObjectRecord>>(
            [.. query.OrderBy(record => record.CleaningObject.Name)]);
    }

    public Task<CleaningObjectRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Records().SingleOrDefault(record => record.CleaningObject.Id == id));

    public Task<CleaningObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(item => item.Id == id));

    public Task<bool> ObjectNumberExistsAsync(string objectNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => item.Id != excludedId
            && string.Equals(item.ObjectNumber, objectNumber, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken) =>
        Task.FromResult(customers.Items.Any(customer => customer.Id == customerId));

    public Task AddAsync(CleaningObject cleaningObject, CancellationToken cancellationToken)
    {
        Items.Add(cleaningObject);
        return Task.CompletedTask;
    }

    public void Remove(CleaningObject cleaningObject) => Items.Remove(cleaningObject);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private IEnumerable<CleaningObjectRecord> Records() => Items.Select(item =>
    {
        var customer = customers.Items.Single(customer => customer.Id == item.CustomerId);
        return new CleaningObjectRecord(item, customer.CustomerNumber, customer.CompanyName);
    });
}

internal sealed class FakeEmployeeRepository(params Employee[] seed) : IEmployeeRepository
{
    public List<Employee> Items { get; } = [.. seed];
    public IReadOnlyCollection<EmployeeContract> Contracts { get; set; } = [];

    public Task<IReadOnlyList<Employee>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        IEnumerable<Employee> query = Items;
        if (search is not null)
        {
            query = query.Where(employee =>
                employee.EmployeeNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || employee.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || employee.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (employee.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (employee.Phone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (employee.MobilePhone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (employee.City?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (isActive.HasValue)
        {
            query = query.Where(employee => employee.IsActive == isActive.Value);
        }

        return Task.FromResult<IReadOnlyList<Employee>>(
            [.. query.OrderBy(employee => employee.LastName).ThenBy(employee => employee.FirstName)]);
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(employee => employee.Id == id));

    public Task<bool> EmployeeNumberExistsAsync(
        string employeeNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(employee => employee.Id != excludedId
            && string.Equals(employee.EmployeeNumber, employeeNumber, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> HasContractsAsync(Guid employeeId, CancellationToken cancellationToken) =>
        Task.FromResult(Contracts.Any(contract => contract.EmployeeId == employeeId));

    public Task AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        Items.Add(employee);
        return Task.CompletedTask;
    }

    public void Remove(Employee employee) => Items.Remove(employee);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeEmployeeContractRepository(
    FakeEmployeeRepository employees,
    params EmployeeContract[] seed) : IEmployeeContractRepository
{
    public List<EmployeeContract> Items { get; } = [.. seed];

    public Task<IReadOnlyList<EmployeeContractRecord>> GetAllAsync(
        string? search,
        bool? isActive,
        Guid? employeeId,
        CancellationToken cancellationToken)
    {
        IEnumerable<EmployeeContractRecord> query = Records();
        if (search is not null)
        {
            query = query.Where(record =>
                record.Contract.ContractNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (record.Contract.EmploymentType?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || record.EmployeeNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || record.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (isActive.HasValue) query = query.Where(record => record.Contract.IsActive == isActive.Value);
        if (employeeId.HasValue) query = query.Where(record => record.Contract.EmployeeId == employeeId.Value);
        return Task.FromResult<IReadOnlyList<EmployeeContractRecord>>(
            [.. query.OrderByDescending(record => record.Contract.StartDate)]);
    }

    public Task<EmployeeContractRecord?> GetRecordByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Records().SingleOrDefault(record => record.Contract.Id == id));

    public Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(contract => contract.Id == id));

    public Task<bool> EmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken) =>
        Task.FromResult(employees.Items.Any(employee => employee.Id == employeeId));

    public Task<bool> ContractNumberExistsAsync(
        string contractNumber,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(contract => contract.Id != excludedId
            && string.Equals(contract.ContractNumber, contractNumber, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> HasOverlappingActiveContractAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate,
        Guid? excludedId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(contract => contract.EmployeeId == employeeId
            && contract.IsActive
            && contract.Id != excludedId
            && (!endDate.HasValue || contract.StartDate <= endDate.Value)
            && (!contract.EndDate.HasValue || startDate <= contract.EndDate.Value)));

    public Task AddAsync(EmployeeContract contract, CancellationToken cancellationToken)
    {
        Items.Add(contract);
        return Task.CompletedTask;
    }

    public void Remove(EmployeeContract contract) => Items.Remove(contract);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private IEnumerable<EmployeeContractRecord> Records() => Items.Select(contract =>
    {
        Employee employee = employees.Items.Single(item => item.Id == contract.EmployeeId);
        return new EmployeeContractRecord(
            contract,
            employee.EmployeeNumber,
            $"{employee.FirstName} {employee.LastName}");
    });
}
