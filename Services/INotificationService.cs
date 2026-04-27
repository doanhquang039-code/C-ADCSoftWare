using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(int userId, string title, string message, string type, string link = null);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}
