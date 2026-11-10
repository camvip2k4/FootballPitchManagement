using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(int accountId, string type, string message)
        {
            var notification = new Notification
            {
                AccountId = accountId,
                Type = type,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int accountId)
        {
            return await _context.Notifications
                .Where(n => n.AccountId == accountId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int accountId)
        {
            return await _context.Notifications
                .CountAsync(n => n.AccountId == accountId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notif = await _context.Notifications.FindAsync(notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}

