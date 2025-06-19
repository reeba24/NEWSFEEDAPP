using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommentsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult AddComment([FromBody] NewsComment comment)
        {
            try
            {
                int newCommentId = 1;

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    conn.Open();

                    string getMaxIdQuery = "SELECT ISNULL(MAX(comment_id), 0) FROM COMMENT";
                    using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            newCommentId = Convert.ToInt32(result) + 1;
                        }
                    }

                    string insertQuery = @"INSERT INTO COMMENT 
                                        (comment_id, news_id, u_id, comments, active, created_time, modified_time) 
                                        VALUES 
                                        (@comment_id, @news_id, @u_id, @comments, 1, GETDATE(), GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@comment_id", newCommentId);
                        cmd.Parameters.AddWithValue("@news_id", comment.news_id);
                        cmd.Parameters.AddWithValue("@u_id", comment.u_id);
                        cmd.Parameters.AddWithValue("@comments", comment.comments);
                        cmd.ExecuteNonQuery();
                    }

                    string nameQuery = "SELECT first_name, last_name FROM USERS WHERE u_id = @uid";
                    SqlCommand nameCmd = new SqlCommand(nameQuery, conn);
                    nameCmd.Parameters.AddWithValue("@uid", comment.u_id);
                    string commenterName = "";

                    using (SqlDataReader reader = nameCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            commenterName = reader["first_name"] + " " + reader["last_name"];
                        }
                    }

                    string authorQuery = "SELECT u_id, news_title FROM NEWS WHERE news_id = @nid";
                    SqlCommand authorCmd = new SqlCommand(authorQuery, conn);
                    authorCmd.Parameters.AddWithValue("@nid", comment.news_id);

                    int authorId = 0;
                    string newsTitle = "";
                    using (SqlDataReader authorReader = authorCmd.ExecuteReader())
                    {
                        if (authorReader.Read())
                        {
                            authorId = (int)authorReader["u_id"];
                            newsTitle = authorReader["news_title"].ToString();
                        }
                    }

                    if (authorId != 0 && authorId != comment.u_id)
                    {
                        string notifText = $"{commenterName} commented on your post: {newsTitle}";

                        string insertNotifQuery = @"
                            INSERT INTO NOTIFICATION 
                            (u_id, notificationtype_id, created_time, notification_text, active)
                            VALUES 
                            (@uid, @typeId, GETDATE(), @text, 1)";

                        SqlCommand notifCmd = new SqlCommand(insertNotifQuery, conn);
                        notifCmd.Parameters.AddWithValue("@uid", authorId);
                        notifCmd.Parameters.AddWithValue("@typeId", 5);
                        notifCmd.Parameters.AddWithValue("@text", notifText);
                        notifCmd.ExecuteNonQuery();
                    }
                }

                return Ok(new { message = "Comment added successfully", comment_id = newCommentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("news/{news_id}")]
        public IActionResult GetCommentsByNewsId(int news_id)
        {
            try
            {
                List<CommentModel> comments = new List<CommentModel>();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            C.comment_id,
                            C.news_id,
                            C.u_id,
                            C.comments,
                            C.created_time,
                            U.first_name,
                            U.last_name
                        FROM COMMENT C
                        JOIN USERS U ON C.u_id = U.u_id
                        WHERE C.news_id = @news_id AND C.active = 1
                        ORDER BY C.created_time ASC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@news_id", news_id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comments.Add(new CommentModel
                            {
                                comment_id = (int)reader["comment_id"],
                                news_id = (int)reader["news_id"],
                                u_id = (int)reader["u_id"],
                                comments = reader["comments"].ToString(),
                                created_time = (DateTime)reader["created_time"],
                                first_name = reader["first_name"].ToString(),
                                last_name = reader["last_name"].ToString()
                            });
                        }
                    }
                }

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
