using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<Notification>> GetNotificationsAsync(int userId)
        {
            var notifications = new List<Notification>();
            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            string query = @"SELECT notification_id, u_id, notificationtype_id, created_time, read_time, notification_text, active 
                             FROM NOTIFICATION 
                             WHERE u_id = @UserId AND active = 1";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notifications.Add(new Notification
                {
                    notification_id = Convert.ToInt32(reader["notification_id"]),
                    u_id = Convert.ToInt32(reader["u_id"]),
                    notificationtype_id = Convert.ToInt32(reader["notificationtype_id"]),
                    created_time = Convert.ToDateTime(reader["created_time"]),
                    read_time = reader["read_time"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["read_time"]),
                    notification_text = reader["notification_text"].ToString(),
                    active = Convert.ToBoolean(reader["active"])
                });
            }

            return notifications;
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE notification_id = @NotificationId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@NotificationId", notificationId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ClearAllAsync(int userId)
        {
            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE u_id = @UserId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
