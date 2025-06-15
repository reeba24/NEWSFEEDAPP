using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EditProfileController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        [HttpGet("GetProfile/{userId}")]
        public IActionResult GetProfile(int userId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("NewsDbConnection");
                EditProfile profile = new EditProfile();
                profile.UserId = userId;
                profile.PreferenceIds = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string userQuery = "SELECT first_name, last_name, about FROM USERS WHERE u_id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(userQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                profile.FirstName = reader["first_name"].ToString();
                                profile.LastName = reader["last_name"].ToString();
                                profile.About = reader["about"]?.ToString();
                            }
                        }
                    }

                    string prefQuery = "SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(prefQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                profile.PreferenceIds.Add(Convert.ToInt32(reader["pref_id"]));
                            }
                        }
                    }
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("Update")]
        public IActionResult UpdateProfile([FromBody] EditProfile profile)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("NewsDbConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateUserQuery = @"
                        UPDATE USERS
                        SET first_name = @FirstName,
                            last_name = @LastName,
                            about = @About,
                            modified_time = GETDATE()
                        WHERE u_id = @UserId";

                    using (SqlCommand cmd = new SqlCommand(updateUserQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", profile.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", profile.LastName);
                        cmd.Parameters.AddWithValue("@About", profile.About ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserId", profile.UserId);
                        cmd.ExecuteNonQuery();
                    }

                    string deletePrefsQuery = "DELETE FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(deletePrefsQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", profile.UserId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var prefId in profile.PreferenceIds)
                    {
                        string insertPrefQuery = @"
                            INSERT INTO USER_PREF_BRIDGE (u_id, pref_id, created_time, modified_time, active)
                            VALUES (@UserId, @PrefId, GETDATE(), GETDATE(), 1)";

                        using (SqlCommand cmd = new SqlCommand(insertPrefQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserId", profile.UserId);
                            cmd.Parameters.AddWithValue("@PrefId", prefId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
