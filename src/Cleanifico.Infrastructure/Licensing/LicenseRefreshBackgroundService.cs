using Cleanifico.Application.Licensing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Licensing;

internal sealed class LicenseRefreshBackgroundService(
    ILicenseRefreshService refreshService,
    IOptions<LicensingOptions> options,
    TimeProvider timeProvider,
    ILogger<LicenseRefreshBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunRefreshAsync(stoppingToken);
        using var timer = new PeriodicTimer(options.Value.RefreshInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunRefreshAsync(stoppingToken);
        }
    }

    private async Task RunRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            LicenseOperationResult result = await refreshService.RefreshAsync(cancellationToken);
            logger.LogInformation(
                "Background license refresh completed with result {ResultCode}.",
                result.Status);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Background license refresh failed; the local signed lease remains unchanged.");
        }
    }
}
