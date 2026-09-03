using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services.Jobs;

public class AutomatedMaintenanceBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomatedMaintenanceBackgroundWorker> _logger;

    private static readonly TimeSpan FastJobInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DailyJobInterval = TimeSpan.FromHours(6);

    public AutomatedMaintenanceBackgroundWorker(
        IServiceProvider serviceProvider,
        ILogger<AutomatedMaintenanceBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automated E-Commerce Maintenance Background Worker started.");

        var lastDailyRun = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobsService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

                    // 1. Fast Job: Release expired inventory reservations and sync low stock alerts
                    await jobsService.ReleaseExpiredStockHoldsAsync(stoppingToken);
                    await jobsService.SyncLowStockAlertsAsync(stoppingToken);

                    // 2. Periodic Maintenance: Deactivate expired coupons and clean stale guest carts
                    if (DateTime.UtcNow - lastDailyRun >= DailyJobInterval)
                    {
                        await jobsService.DeactivateExpiredCouponsAsync(stoppingToken);
                        await jobsService.CleanupAbandonedGuestCartsAsync(30, stoppingToken);
                        lastDailyRun = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred during automated background maintenance cycle.");
            }

            try
            {
                await Task.Delay(FastJobInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Automated E-Commerce Maintenance Background Worker stopped.");
    }
}
