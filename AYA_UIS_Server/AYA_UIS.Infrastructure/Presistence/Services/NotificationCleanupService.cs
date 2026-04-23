using Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Presistence.Services
{
    /// <summary>
    /// Background service that purges notifications older than 4 days.
    /// Runs every 6 hours to keep the Notifications table lean.
    /// </summary>
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationCleanupService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
        private const int MaxAgeDays = 4;

        public NotificationCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait a short while so the application is fully started before the first run
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var cutoff = DateTime.UtcNow.AddDays(-MaxAgeDays);
                    await uow.Notifications.DeleteOlderThanAsync(cutoff);
                    _logger.LogInformation(
                        "NotificationCleanup: removed notifications older than {Cutoff:u}", cutoff);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NotificationCleanup: error during cleanup");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
