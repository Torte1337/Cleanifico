using System.Net;

namespace Cleanifico.Api.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        await using var host = await ApiTestHost.StartAsync();

        using var response = await host.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
