using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FeedController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("status")]
        public IActionResult GetFeedStatus([FromQuery] int userId)
        {
            string connStr = _configuration.GetConnectionString("NewsDbConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string prefQuery = "SELECT COUNT(*) FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                using (SqlCommand prefCmd = new SqlCommand(prefQuery, conn))
                {
                    prefCmd.Parameters.AddWithValue("@UserId", userId);
                    int prefCount = (int)prefCmd.ExecuteScalar();
                    if (prefCount == 0)
                        return Ok(new { status = "noprefs" });
                }

                string followQuery = "SELECT COUNT(*) FROM FOLLOWED WHERE followed_by_uid = @UserId AND activeind = 1";
                using (SqlCommand followCmd = new SqlCommand(followQuery, conn))
                {
                    followCmd.Parameters.AddWithValue("@UserId", userId);
                    int followCount = (int)followCmd.ExecuteScalar();

                    if (followCount == 0)
                    {
                        string prefNewsQuery = @"
                            SELECT * FROM NEWS 
                            WHERE pref_id IN (
                                SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId
                            ) AND active = 1
                            ORDER BY created_time DESC";

                        using (SqlCommand newsCmd = new SqlCommand(prefNewsQuery, conn))
                        {
                            newsCmd.Parameters.AddWithValue("@UserId", userId);
                            SqlDataReader reader = newsCmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                return Ok(new { status = "ready" });
                            }
                            else
                            {
                                return Ok(new { status = "nonews" });
                            }
                        }
                    }
                }

                string followedNewsQuery = @"
                    SELECT COUNT(*) 
                    FROM NEWS 
                    WHERE u_id IN (
                        SELECT followed_uid 
                        FROM FOLLOWED 
                        WHERE followed_by_uid = @UserId AND activeind = 1
                    ) AND active = 1";

                using (SqlCommand newsCmd = new SqlCommand(followedNewsQuery, conn))
                {
                    newsCmd.Parameters.AddWithValue("@UserId", userId);
                    int newsCount = (int)newsCmd.ExecuteScalar();
                    if (newsCount == 0)
                        return Ok(new { status = "nonews" });
                }

                return Ok(new { status = "ready" });
            }
        }
    }
}

