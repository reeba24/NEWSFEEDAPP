using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public NotificationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetNotifications/{userId}")]
        public IActionResult GetNotifications(int userId)
        {
            List<Notification> notifications = new List<Notification>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                conn.Open();
                string query = @"SELECT notification_id, u_id, notificationtype_id, created_time, read_time, notification_text, active 
                             FROM NOTIFICATION 
                             WHERE u_id = @UserId AND active = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        notifications.Add(new Notification
                        {
                            notification_id = Convert.ToInt32(reader["notification_id"]),
                            u_id = Convert.ToInt32(reader["u_id"]),
                            notificationtype_id = Convert.ToInt32(reader["notificationtype_id"]),
                            created_time = Convert.ToDateTime(reader["created_time"]),
                            read_time = reader["read_time"] == DBNull.Value? (DateTime?)null : Convert.ToDateTime(reader["read_time"]),
                            notification_text = reader["notification_text"].ToString(),
                            active = Convert.ToBoolean(reader["active"])
                        });
                    }
                }
            }

            return Ok(notifications);
        }

        [HttpPost("MarkAsRead/{notificationId}")]
        public IActionResult MarkAsRead(int notificationId)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                conn.Open();
                string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE notification_id = @NotificationId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPost("ClearAll/{userId}")]
        public IActionResult ClearAll(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                conn.Open();
                string query = @"UPDATE NOTIFICATION 
                             SET active = 0, read_time = GETDATE() 
                             WHERE u_id = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { message = "All notifications cleared" });
        }
    }
}
