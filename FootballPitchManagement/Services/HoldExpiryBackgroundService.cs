using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class HoldExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HoldExpiryBackgroundService> _logger;

        public HoldExpiryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<HoldExpiryBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HoldExpiryBackgroundService (System Timer UC40) started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var now = DateTime.UtcNow;

                        // UC40: Expire unconfirmed holds whose hold time has passed
                        var expiredBookings = await context.Bookings
                            .Where(b => (b.Status == BookingStatus.PENDING_PAYMENT || b.Status == BookingStatus.PAYMENT_FAILED)
                                        && b.HoldExpiresAt < now)
                            .ToListAsync(stoppingToken);

                        if (expiredBookings.Any())
                        {
                            foreach (var b in expiredBookings)
                            {
                                b.Status = BookingStatus.EXPIRED;
                                _logger.LogInformation("Booking {Code} expired due to hold timeout.", b.BookingCode);

                                if (b.AccountId.HasValue)
                                {
                                    await notificationService.SendNotificationAsync(
                                        b.AccountId.Value,
                                        "HOLD_EXPIRED",
                                        $"Your hold on booking {b.BookingCode} has expired. The time slot was released.");
                                }
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in HoldExpiryBackgroundService.");
                }

                // Check every 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}

