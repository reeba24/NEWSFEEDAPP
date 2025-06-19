using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using newsapp.Models;
using NewsApp.Repository.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;

namespace newsapp.Controllers
{
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
        public async Task<IActionResult> CreatePost([FromForm] NewsPost post)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    await con.OpenAsync();

                    string getNewsIdQuery = "SELECT ISNULL(MAX(news_id), 0) + 1 FROM NEWS";
                    SqlCommand newsIdCmd = new SqlCommand(getNewsIdQuery, con);
                    int nextNewsId = (int)await newsIdCmd.ExecuteScalarAsync();

                    string getMediaIdQuery = "SELECT ISNULL(MAX(media_id), 0) + 1 FROM MEDIA";
                    SqlCommand mediaIdCmd = new SqlCommand(getMediaIdQuery, con);
                    int nextMediaId = (int)await mediaIdCmd.ExecuteScalarAsync();

                    string getPrefIdQuery = @"
                        SELECT pref_id FROM PREFERENCES 
                        WHERE LTRIM(RTRIM(LOWER(pref_name))) = LTRIM(RTRIM(LOWER(@prefName)))";

                    SqlCommand prefCmd = new SqlCommand(getPrefIdQuery, con);
                    prefCmd.Parameters.AddWithValue("@prefName", post.pref_name?.Trim() ?? "");
                    object prefIdObj = await prefCmd.ExecuteScalarAsync();

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
                    await newsCmd.ExecuteNonQueryAsync();

                    byte[] imageBytes = null;
                    if (post.image != null && post.image.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await post.image.CopyToAsync(ms);
                            imageBytes = ms.ToArray();
                        }
                    }

                    await new SqlCommand("SET IDENTITY_INSERT MEDIA ON", con).ExecuteNonQueryAsync();

                    string insertMediaQuery = @"
                        INSERT INTO MEDIA (media_id, news_id, image)
                        VALUES (@mediaId, @newsId, @image)";

                    SqlCommand mediaCmd = new SqlCommand(insertMediaQuery, con);
                    mediaCmd.Parameters.AddWithValue("@mediaId", nextMediaId);
                    mediaCmd.Parameters.AddWithValue("@newsId", nextNewsId);
                    mediaCmd.Parameters.AddWithValue("@image", (object?)imageBytes ?? DBNull.Value);
                    await mediaCmd.ExecuteNonQueryAsync();

                    await new SqlCommand("SET IDENTITY_INSERT MEDIA OFF", con).ExecuteNonQueryAsync();

                    string getAuthorNameQuery = "SELECT first_name, last_name FROM USERS WHERE u_id = @uid";
                    SqlCommand nameCmd = new SqlCommand(getAuthorNameQuery, con);
                    nameCmd.Parameters.AddWithValue("@uid", post.u_id);
                    string fullName = "";

                    using (SqlDataReader reader = await nameCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            fullName = reader["first_name"] + " " + reader["last_name"];
                        }
                    }

                    string followersQuery = "SELECT followed_by_uid FROM FOLLOWED WHERE followed_uid = @author";
                    SqlCommand followersCmd = new SqlCommand(followersQuery, con);
                    followersCmd.Parameters.AddWithValue("@author", post.u_id);

                    var followerIds = new List<int>();
                    using (SqlDataReader followerReader = await followersCmd.ExecuteReaderAsync())
                    {
                        while (await followerReader.ReadAsync())
                        {
                            followerIds.Add(Convert.ToInt32(followerReader["followed_by_uid"]));
                        }
                    }

                    if (followerIds.Count > 0)
                    {
                        string insertNotifQuery = @"
                            INSERT INTO NOTIFICATION (u_id, notificationtype_id, created_time, notification_text, active)
                            VALUES (@uid, @typeId, GETDATE(), @text, 1)";

                        foreach (int fid in followerIds)
                        {
                            SqlCommand notifCmd = new SqlCommand(insertNotifQuery, con);
                            notifCmd.Parameters.AddWithValue("@uid", fid);
                            notifCmd.Parameters.AddWithValue("@typeId", 2); 
                            notifCmd.Parameters.AddWithValue("@text", $"{fullName} created a new post: {post.news_title}");
                            await notifCmd.ExecuteNonQueryAsync();
                        }
                    }

                    return Ok(new { message = "Post created successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }
    }
}
