using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SavedController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("save")]
        public IActionResult SaveNews([FromBody] Saved saved)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM SAVED WHERE u_id = @u_id AND news_id = @news_id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@u_id", saved.u_id);
                    checkCmd.Parameters.AddWithValue("@news_id", saved.news_id);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        string updateQuery = "UPDATE SAVED SET active = 1, modified_time = GETDATE() WHERE u_id = @u_id AND news_id = @news_id";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@u_id", saved.u_id);
                        updateCmd.Parameters.AddWithValue("@news_id", saved.news_id);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        string getMaxIdQuery = "SELECT ISNULL(MAX(save_id), 0) + 1 FROM SAVED";
                        SqlCommand maxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                        int newSaveId = (int)maxIdCmd.ExecuteScalar();

                        string insertQuery = @"
                            INSERT INTO SAVED (save_id, news_id, u_id, active, created_time, modified_time)
                            VALUES (@save_id, @news_id, @u_id, 1, GETDATE(), GETDATE())";

                        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                        insertCmd.Parameters.AddWithValue("@save_id", newSaveId);
                        insertCmd.Parameters.AddWithValue("@u_id", saved.u_id);
                        insertCmd.Parameters.AddWithValue("@news_id", saved.news_id);
                        insertCmd.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Saved!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("status")]
        public IActionResult GetSavedStatus([FromQuery] int news_id, [FromQuery] int u_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    conn.Open();

                    string query = "SELECT active FROM SAVED WHERE news_id = @news_id AND u_id = @u_id AND active = 1";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@news_id", news_id);
                    cmd.Parameters.AddWithValue("@u_id", u_id);

                    var result = cmd.ExecuteScalar();
                    bool hasSaved = result != null;

                    return Ok(new { hasSaved });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getsavednews/{u_id}")]
        public IActionResult GetSavedNews(int u_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    conn.Open();

                    HashSet<int> followedAuthorIds = new HashSet<int>();
                    using (SqlCommand followCmd = new SqlCommand("SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @u_id", conn))
                    {
                        followCmd.Parameters.AddWithValue("@u_id", u_id);
                        using (SqlDataReader followReader = followCmd.ExecuteReader())
                        {
                            while (followReader.Read())
                            {
                                followedAuthorIds.Add((int)followReader["followed_uid"]);
                            }
                        }
                    }

                    string sql = @"
                        SELECT 
                            n.news_id, 
                            n.news_title, 
                            n.contents, 
                            m.image,
                            u.first_name, 
                            u.last_name,
                            u.u_id,
                            ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                            ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                            n.created_time
                        FROM SAVED s
                        JOIN NEWS n ON s.news_id = n.news_id
                        LEFT JOIN MEDIA m ON n.news_id = m.news_id
                        JOIN USERS u ON n.u_id = u.u_id
                        LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                        WHERE s.u_id = @u_id AND s.active = 1 AND n.active = 1
                        GROUP BY 
                            n.news_id, 
                            n.news_title, 
                            n.contents, 
                            m.image,
                            u.first_name, 
                            u.last_name,
                            u.u_id,
                            n.created_time";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@u_id", u_id);

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TileData> savedNewsList = new List<TileData>();

                    while (reader.Read())
                    {
                        int authorId = (int)reader["u_id"];
                        var imageBytes = reader["image"] as byte[];
                        string imageBase64 = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                        savedNewsList.Add(new TileData
                        {
                            news_id = (int)reader["news_id"],
                            news_title = reader["news_title"].ToString(),
                            contents = reader["contents"].ToString(),
                            image = imageBytes,
                            imageBase64 = imageBase64,
                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            u_id = authorId,
                            likes = (int)reader["likes"],
                            unlikes = (int)reader["unlikes"],
                            created_time = (DateTime)reader["created_time"],
                            isFollowed = followedAuthorIds.Contains(authorId),
                            comments = new List<CommentModel>()
                        });
                    }
                    reader.Close();

                    if (savedNewsList.Count == 0)
                        return Ok(savedNewsList);

                    string commentSql = $@"
                        SELECT comment_id, news_id, u_id, comments, created_time 
                        FROM COMMENT 
                        WHERE active = 1 AND news_id IN ({string.Join(",", savedNewsList.Select(n => n.news_id))})
                        ORDER BY created_time DESC";

                    SqlCommand commentCmd = new SqlCommand(commentSql, conn);
                    SqlDataReader commentReader = commentCmd.ExecuteReader();

                    Dictionary<int, List<CommentModel>> commentLookup = new();

                    while (commentReader.Read())
                    {
                        var comment = new CommentModel
                        {
                            comment_id = (int)commentReader["comment_id"],
                            news_id = (int)commentReader["news_id"],
                            u_id = (int)commentReader["u_id"],
                            comments = commentReader["comments"].ToString(),
                            created_time = (DateTime)commentReader["created_time"]
                        };

                        if (!commentLookup.ContainsKey(comment.news_id))
                            commentLookup[comment.news_id] = new List<CommentModel>();

                        commentLookup[comment.news_id].Add(comment);
                    }

                    foreach (var news in savedNewsList)
                    {
                        if (commentLookup.ContainsKey(news.news_id))
                            news.comments = commentLookup[news.news_id];
                    }

                    return Ok(savedNewsList);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
