using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task ClearAllAsync(int userId);
    }
}
