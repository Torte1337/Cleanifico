using System.Net;
using Cleanifico.Api;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Cleanifico.Api.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        await using var app = ApiApplication.Build(["--environment", "Testing"]);
        app.Urls.Add("http://127.0.0.1:0");

        await app.StartAsync();

        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
            var address = Assert.Single(addresses ?? []);

            using var client = new HttpClient { BaseAddress = new Uri(address) };
            using var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
