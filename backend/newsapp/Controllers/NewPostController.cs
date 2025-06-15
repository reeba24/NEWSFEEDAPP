namespace newsapp.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using newsapp.Models;
using NewsApp.Repository.Models;

[Route("api/[controller]")]
[ApiController]
public class NewPostController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public NewPostController(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    [HttpPost]
    public IActionResult CreatePost([FromBody] NewsPost post)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                con.Open();


                string getNewsIdQuery = "SELECT ISNULL(MAX(news_id), 0) + 1 FROM NEWS";
                SqlCommand newsIdCmd = new SqlCommand(getNewsIdQuery, con);
                int nextNewsId = (int)newsIdCmd.ExecuteScalar();

                string getMediaIdQuery = "SELECT ISNULL(MAX(media_id), 0) + 1 FROM MEDIA";
                SqlCommand mediaIdCmd = new SqlCommand(getMediaIdQuery, con);
                int nextMediaId = (int)mediaIdCmd.ExecuteScalar();

                string getPrefIdQuery = @"
                    SELECT pref_id FROM PREFERENCES 
                    WHERE LTRIM(RTRIM(LOWER(pref_name))) = LTRIM(RTRIM(LOWER(@prefName)))";

                SqlCommand prefCmd = new SqlCommand(getPrefIdQuery, con);
                prefCmd.Parameters.AddWithValue("@prefName", post.pref_name?.Trim() ?? "");
                object prefIdObj = prefCmd.ExecuteScalar();

                if (prefIdObj == null)
                    return BadRequest(new { message = "Invalid preference/category name." });

                int prefId = (int)prefIdObj;


                string insertNewsQuery = @"
                    INSERT INTO NEWS (news_id, media_id, pref_id, news_title, contents, u_id, active, created_time)
                    VALUES (@newsId, @mediaId, @prefId, @title, @content, @uid, @active, @createdTime)";

                SqlCommand newsCmd = new SqlCommand(insertNewsQuery, con);
                newsCmd.Parameters.AddWithValue("@newsId", nextNewsId);
                newsCmd.Parameters.AddWithValue("@mediaId", nextMediaId);
                newsCmd.Parameters.AddWithValue("@prefId", prefId);
                newsCmd.Parameters.AddWithValue("@title", post.news_title);
                newsCmd.Parameters.AddWithValue("@content", post.contents);
                newsCmd.Parameters.AddWithValue("@uid", post.u_id);
                newsCmd.Parameters.AddWithValue("@active", post.active);
                newsCmd.Parameters.AddWithValue("@createdTime", DateTime.Now);
                newsCmd.ExecuteNonQuery();


                string insertMediaQuery = @"
                    INSERT INTO MEDIA (media_id, news_id, image)
                    VALUES (@mediaId, @newsId, NULL)";

                SqlCommand mediaCmd = new SqlCommand(insertMediaQuery, con);
                mediaCmd.Parameters.AddWithValue("@mediaId", nextMediaId);
                mediaCmd.Parameters.AddWithValue("@newsId", nextNewsId);
                mediaCmd.ExecuteNonQuery();

                return Ok(new { message = "Post created successfully" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNews()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                await con.OpenAsync();

                string query = @"
                SELECT N.news_id, N.news_title, N.contents, N.created_time, 
                       P.pref_name, M.image,
                       U.first_name, U.last_name,
                       ISNULL(L.likes, 0) AS likes, ISNULL(L.unlikes, 0) AS unlikes
                FROM NEWS N
                INNER JOIN PREFERENCES P ON N.pref_id = P.pref_id
                LEFT JOIN MEDIA M ON N.news_id = M.news_id
                INNER JOIN USERS U ON N.u_id = U.u_id
                LEFT JOIN (
                    SELECT news_id, 
                           SUM(CASE WHEN like_status = 1 THEN 1 ELSE 0 END) AS likes,
                           SUM(CASE WHEN like_status = 0 THEN 1 ELSE 0 END) AS unlikes
                    FROM LIKEUNLIKE
                    GROUP BY news_id
                ) L ON N.news_id = L.news_id
                WHERE N.active = 1
                ORDER BY N.created_time DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                var newsList = new List<object>();

                while (await reader.ReadAsync())
                {
                    byte[] imageBytes = reader["image"] != DBNull.Value ? (byte[])reader["image"] : null;
                    string base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                    int newsId = (int)reader["news_id"];

                    // Fetch comments for each news_id
                    List<object> comments = await GetComments(con, newsId);

                    newsList.Add(new
                    {
                        news_id = newsId,
                        news_title = (string)reader["news_title"],
                        contents = (string)reader["contents"],
                        created_time = (DateTime)reader["created_time"],
                        pref_name = (string)reader["pref_name"],
                        imageBase64 = base64Image,
                        first_name = (string)reader["first_name"],
                        last_name = (string)reader["last_name"],
                        likes = (int)reader["likes"],
                        unlikes = (int)reader["unlikes"],
                        comments = comments
                    });
                }

                return Ok(newsList);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error: " + ex.Message });
        }
    }

    private async Task<List<object>> GetComments(SqlConnection con, int newsId)
    {
        string commentQuery = @"SELECT comment_id, news_id, u_id, comments, created_time FROM COMMENT WHERE news_id = @newsId";
        SqlCommand cmd = new SqlCommand(commentQuery, con);
        cmd.Parameters.AddWithValue("@newsId", newsId);

        var commentList = new List<object>();
        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                commentList.Add(new
                {
                    comment_id = (int)reader["comment_id"],
                    news_id = (int)reader["news_id"],
                    u_id = (int)reader["u_id"],
                    comments = (string)reader["comments"],
                    created_time = (DateTime)reader["created_time"]
                });
            }
        }
        return commentList;
    }
}
