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
                }

                return Ok(new { message = "Comment added successfully", comment_id = newCommentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
