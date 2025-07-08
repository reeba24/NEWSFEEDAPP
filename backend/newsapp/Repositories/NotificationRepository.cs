using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;
using System.Data;

namespace newsapp.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDataManager _dataManager;

        public NotificationRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<List<Notification>> GetNotificationsAsync(int userId)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string query = @"SELECT notification_id, u_id, notificationtype_id, created_time, read_time, notification_text, active 
                             FROM NOTIFICATION 
                             WHERE u_id = @UserId AND active = 1";

            var notifications = await conn.QueryAsync<Notification>(query, new { UserId = userId });

            return notifications.ToList();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE notification_id = @NotificationId";

            await conn.ExecuteAsync(query, new { NotificationId = notificationId });
        }

        public async Task ClearAllAsync(int userId)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE u_id = @UserId";

            await conn.ExecuteAsync(query, new { UserId = userId });
        }
    }
}
