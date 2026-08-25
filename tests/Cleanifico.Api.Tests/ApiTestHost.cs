using Cleanifico.Api;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Domain.CleaningTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Cleanifico.Api.Tests;

internal sealed class ApiTestHost : IAsyncDisposable
{
    private readonly WebApplication application;

    private ApiTestHost(
        WebApplication application,
        HttpClient client,
        FakeCleaningTypeRepository repository)
    {
        this.application = application;
        Client = client;
        Repository = repository;
    }

    public HttpClient Client { get; }

    public FakeCleaningTypeRepository Repository { get; }

    public static async Task<ApiTestHost> StartAsync(params CleaningType[] seed)
    {
        var repository = new FakeCleaningTypeRepository(seed);
        var application = ApiApplication.Build(
            [
                "--environment", "Testing",
                "--ConnectionStrings:Cleanifico",
                "Server=localhost;Database=cleanifico_api_test;User=test;Password=not-used"
            ],
            services => services.AddSingleton<ICleaningTypeRepository>(repository));

        application.Urls.Add("http://127.0.0.1:0");
        await application.StartAsync();

        var server = application.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var address = Assert.Single(addresses ?? []);
        var client = new HttpClient { BaseAddress = new Uri(address) };

        return new ApiTestHost(application, client, repository);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.StopAsync();
        await application.DisposeAsync();
    }
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
