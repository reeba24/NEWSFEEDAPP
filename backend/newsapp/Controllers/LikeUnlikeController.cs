using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeUnlikeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LikeUnlikeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("like")]
        public IActionResult Like([FromBody] Like like)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM LIKE_UNLIKE WHERE u_id = @u_id AND news_id = @news_id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@u_id", like.u_id);
                    checkCmd.Parameters.AddWithValue("@news_id", like.news_id);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        string updateQuery = @"
                            UPDATE LIKE_UNLIKE 
                            SET likes = 1, unlikes = 0, like_modified = 1, modified_time = GETDATE() 
                            WHERE u_id = @u_id AND news_id = @news_id";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@u_id", like.u_id);
                        updateCmd.Parameters.AddWithValue("@news_id", like.news_id);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        string getMaxIdQuery = "SELECT ISNULL(MAX(like_id), 0) + 1 FROM LIKE_UNLIKE";
                        SqlCommand maxIdCmd = new SqlCommand(getMaxIdQuery, connection);
                        int newLikeId = (int)maxIdCmd.ExecuteScalar();

                        string insertQuery = @"
                            INSERT INTO LIKE_UNLIKE (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) 
                            VALUES (@like_id, @news_id, @u_id, 1, 0, 1, GETDATE())";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@like_id", newLikeId);
                        insertCmd.Parameters.AddWithValue("@news_id", like.news_id);
                        insertCmd.Parameters.AddWithValue("@u_id", like.u_id);
                        insertCmd.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Liked successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("unlike")]
        public IActionResult Unlike([FromBody] Like like)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    connection.Open();

                    string checkQuery = "SELECT like_modified FROM LIKE_UNLIKE WHERE u_id = @u_id AND news_id = @news_id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@u_id", like.u_id);
                    checkCmd.Parameters.AddWithValue("@news_id", like.news_id);

                    var likeModifiedObj = checkCmd.ExecuteScalar();

                    if (likeModifiedObj != null)
                    {
                        bool likeModified = Convert.ToBoolean(likeModifiedObj);

                        string updateQuery;
                        if (likeModified)
                        {
                            updateQuery = @"
                                UPDATE LIKE_UNLIKE 
                                SET likes = 0, unlikes = 1, like_modified = 0, modified_time = GETDATE() 
                                WHERE u_id = @u_id AND news_id = @news_id";
                        }
                        else
                        {
                            updateQuery = @"
                                UPDATE LIKE_UNLIKE 
                                SET unlikes = 1, modified_time = GETDATE() 
                                WHERE u_id = @u_id AND news_id = @news_id";
                        }

                        SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@u_id", like.u_id);
                        updateCmd.Parameters.AddWithValue("@news_id", like.news_id);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        string getMaxIdQuery = "SELECT ISNULL(MAX(like_id), 0) + 1 FROM LIKE_UNLIKE";
                        SqlCommand maxIdCmd = new SqlCommand(getMaxIdQuery, connection);
                        int newLikeId = (int)maxIdCmd.ExecuteScalar();

                        string insertQuery = @"
                            INSERT INTO LIKE_UNLIKE (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) 
                            VALUES (@like_id, @news_id, @u_id, 0, 1, 0, GETDATE())";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@like_id", newLikeId);
                        insertCmd.Parameters.AddWithValue("@news_id", like.news_id);
                        insertCmd.Parameters.AddWithValue("@u_id", like.u_id);
                        insertCmd.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Unliked successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus([FromQuery] int news_id, [FromQuery] int u_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    connection.Open();

                    string query = "SELECT like_modified, likes, unlikes FROM LIKE_UNLIKE WHERE news_id = @news_id AND u_id = @u_id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@news_id", news_id);
                    cmd.Parameters.AddWithValue("@u_id", u_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool likeModified = Convert.ToBoolean(reader["like_modified"]);
                            int likes = Convert.ToInt32(reader["likes"]);
                            int unlikes = Convert.ToInt32(reader["unlikes"]);

                            return Ok(new
                            {
                                hasLiked = likeModified && likes > 0,
                                hasUnliked = !likeModified && unlikes > 0
                            });
                        }
                        else
                        {
                            return Ok(new { hasLiked = false, hasUnliked = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
