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
                        string updateQuery = "UPDATE LIKE_UNLIKE SET likes = likes + 1, like_modified = 1, modified_time = GETDATE() WHERE u_id = @u_id AND news_id = @news_id";
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

                        string insertQuery = "INSERT INTO LIKE_UNLIKE (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) VALUES (@like_id, @news_id, @u_id, 1, 0, 1, GETDATE())";
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

                    string checkQuery = "SELECT like_modified, likes, unlikes FROM LIKE_UNLIKE WHERE u_id = @u_id AND news_id = @news_id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@u_id", like.u_id);
                    checkCmd.Parameters.AddWithValue("@news_id", like.news_id);

                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool likeModified = Convert.ToBoolean(reader["like_modified"]);
                            int currentLikes = Convert.ToInt32(reader["likes"]);
                            int currentUnlikes = Convert.ToInt32(reader["unlikes"]);

                            reader.Close();

                            if (likeModified)
                            {
                                string updateQuery = @"
                                    UPDATE LIKE_UNLIKE
                                    SET likes = @likes, unlikes = @unlikes, like_modified = 0, modified_time = GETDATE()
                                    WHERE u_id = @u_id AND news_id = @news_id";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                                updateCmd.Parameters.AddWithValue("@likes", Math.Max(currentLikes - 1, 0));
                                updateCmd.Parameters.AddWithValue("@unlikes", currentUnlikes + 1);
                                updateCmd.Parameters.AddWithValue("@u_id", like.u_id);
                                updateCmd.Parameters.AddWithValue("@news_id", like.news_id);
                                updateCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                string updateQuery = @"
                                    UPDATE LIKE_UNLIKE
                                    SET unlikes = @unlikes, modified_time = GETDATE()
                                    WHERE u_id = @u_id AND news_id = @news_id";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                                updateCmd.Parameters.AddWithValue("@unlikes", currentUnlikes + 1);
                                updateCmd.Parameters.AddWithValue("@u_id", like.u_id);
                                updateCmd.Parameters.AddWithValue("@news_id", like.news_id);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string getMaxIdQuery = "SELECT ISNULL(MAX(like_id), 0) + 1 FROM LIKE_UNLIKE";
                            SqlCommand maxIdCmd = new SqlCommand(getMaxIdQuery, connection);
                            int newLikeId = (int)maxIdCmd.ExecuteScalar();

                            string insertQuery = "INSERT INTO LIKE_UNLIKE (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) VALUES (@like_id, @news_id, @u_id, 0, 1, 0, GETDATE())";
                            SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                            insertCmd.Parameters.AddWithValue("@like_id", newLikeId);
                            insertCmd.Parameters.AddWithValue("@news_id", like.news_id);
                            insertCmd.Parameters.AddWithValue("@u_id", like.u_id);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    return Ok(new { message = "Unliked successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
