using FootballPitchManagement.Models.Entities;

namespace FootballPitchManagement.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int accountId, string type, string message);
        Task<List<Notification>> GetUserNotificationsAsync(int accountId);
        Task<int> GetUnreadCountAsync(int accountId);
        Task MarkAsReadAsync(int notificationId);
    }
}

