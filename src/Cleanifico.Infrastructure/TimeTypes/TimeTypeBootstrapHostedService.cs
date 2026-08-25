using Cleanifico.Application.TimeTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.TimeTypes;

public sealed class TimeTypeBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<TimeTypeBootstrapOptions> options,
    ILogger<TimeTypeBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ITimeTypeInitializer>()
            .InitializeAsync(cancellationToken);
        logger.LogInformation("Time type standard data initialization has been checked.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
