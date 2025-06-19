using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FollowController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult FollowUser([FromBody] Follow follow)
        {
            if (follow.FollowedByUid <= 0 || follow.FollowedUid <= 0)
            {
                return BadRequest(new { message = "Invalid user ID(s)." });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("NewsDbConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check existing follow status
                    string checkQuery = @"
                        SELECT activeind FROM FOLLOWED
                        WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@FollowedByUid", follow.FollowedByUid);
                    checkCmd.Parameters.AddWithValue("@FollowedUid", follow.FollowedUid);
                    object result = checkCmd.ExecuteScalar();

                    bool isFollowed = result != null && Convert.ToInt32(result) == 1;

                    string followerNameQuery = "SELECT first_name, last_name FROM USERS WHERE u_id = @uid";
                    SqlCommand nameCmd = new SqlCommand(followerNameQuery, connection);
                    nameCmd.Parameters.AddWithValue("@uid", follow.FollowedByUid);
                    string followerName = "";

                    using (SqlDataReader reader = nameCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            followerName = reader["first_name"] + " " + reader["last_name"];
                        }
                    }

                    string notifText = "";
                    int notificationType = 0;

                    if (!isFollowed)
                    {
                        string insertOrUpdate = @"
                            IF EXISTS (
                                SELECT 1 FROM FOLLOWED WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid
                            )
                                UPDATE FOLLOWED SET activeind = 1 
                                WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid
                            ELSE
                                INSERT INTO FOLLOWED (followed_by_uid, followed_uid, activeind)
                                VALUES (@FollowedByUid, @FollowedUid, 1)";

                        SqlCommand cmd = new SqlCommand(insertOrUpdate, connection);
                        cmd.Parameters.AddWithValue("@FollowedByUid", follow.FollowedByUid);
                        cmd.Parameters.AddWithValue("@FollowedUid", follow.FollowedUid);
                        cmd.ExecuteNonQuery();

                        notifText = $"{followerName} started following you.";
                        notificationType = 3;
                    }
                    else
                    {
                      
                        string unfollowQuery = @"
                            UPDATE FOLLOWED 
                            SET activeind = 0 
                            WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid";

                        SqlCommand cmd = new SqlCommand(unfollowQuery, connection);
                        cmd.Parameters.AddWithValue("@FollowedByUid", follow.FollowedByUid);
                        cmd.Parameters.AddWithValue("@FollowedUid", follow.FollowedUid);
                        cmd.ExecuteNonQuery();

                        notifText = $"{followerName} unfollowed you.";
                        notificationType = 4; 
                    }

                    if (!string.IsNullOrEmpty(followerName))
                    {
                        string insertNotifQuery = @"
                            INSERT INTO NOTIFICATION 
                            (u_id, notificationtype_id, created_time, notification_text, active)
                            VALUES 
                            (@uid, @typeId, GETDATE(), @text, 1)";

                        SqlCommand notifCmd = new SqlCommand(insertNotifQuery, connection);
                        notifCmd.Parameters.AddWithValue("@uid", follow.FollowedUid);
                        notifCmd.Parameters.AddWithValue("@typeId", notificationType);
                        notifCmd.Parameters.AddWithValue("@text", notifText);
                        notifCmd.ExecuteNonQuery();
                    }

                    string message = isFollowed ? "Unfollowed successfully." : "Followed successfully.";
                    return Ok(new { message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }
    }
}
