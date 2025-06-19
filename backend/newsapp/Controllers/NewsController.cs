using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public NewsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("all")]
        public IActionResult GetAllNews([FromQuery] int userId)
        {
            var newsList = new List<TileData>();
            var followedUids = new HashSet<int>();
            var preferredPrefIds = new HashSet<int>();

            string connStr = _configuration.GetConnectionString("NewsDbConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string followedQuery = @"SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @UserId AND activeind = 1";
                using (SqlCommand cmd = new SqlCommand(followedQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            followedUids.Add((int)reader["followed_uid"]);
                    }
                }

                string prefQuery = @"SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                using (SqlCommand cmd = new SqlCommand(prefQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            preferredPrefIds.Add((int)reader["pref_id"]);
                    }
                }

                string newsSql = @"
                    SELECT 
                        n.news_id, n.news_title, n.contents, m.image,
                        u.u_id, u.first_name, u.last_name,
                        ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                        ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                        n.created_time, n.pref_id
                    FROM NEWS n
                    LEFT JOIN MEDIA m ON n.news_id = m.news_id
                    LEFT JOIN USERS u ON n.u_id = u.u_id
                    LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                    WHERE n.active = 1
                    GROUP BY 
                        n.news_id, n.news_title, n.contents, m.image,
                        u.u_id, u.first_name, u.last_name,
                        n.created_time, n.pref_id";

                using (SqlCommand cmd = new SqlCommand(newsSql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var authorId = (int)reader["u_id"];
                        var prefId = (int)reader["pref_id"];

                        if (!followedUids.Contains(authorId) && !preferredPrefIds.Contains(prefId))
                            continue;

                        var imageBytes = reader["image"] as byte[];
                        string base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                        newsList.Add(new TileData
                        {
                            news_id = (int)reader["news_id"],
                            news_title = reader["news_title"].ToString(),
                            contents = reader["contents"].ToString(),
                            image = imageBytes,
                            imageBase64 = base64Image,
                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            u_id = authorId,
                            likes = (int)reader["likes"],
                            unlikes = (int)reader["unlikes"],
                            created_time = (DateTime)reader["created_time"],
                            comments = new List<CommentModel>(),
                            isFollowed = followedUids.Contains(authorId)
                        });
                    }
                }

                if (newsList.Count > 0)
                {
                    string commentSql = $@"
                        SELECT comment_id, news_id, u_id, comments, created_time 
                        FROM COMMENT 
                        WHERE active = 1 AND news_id IN ({string.Join(",", newsList.Select(n => n.news_id))})
                        ORDER BY created_time DESC";

                    using (SqlCommand cmd = new SqlCommand(commentSql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var commentLookup = new Dictionary<int, List<CommentModel>>();

                        while (reader.Read())
                        {
                            var comment = new CommentModel
                            {
                                comment_id = (int)reader["comment_id"],
                                news_id = (int)reader["news_id"],
                                u_id = (int)reader["u_id"],
                                comments = reader["comments"].ToString(),
                                created_time = (DateTime)reader["created_time"]
                            };

                            if (!commentLookup.ContainsKey(comment.news_id))
                                commentLookup[comment.news_id] = new List<CommentModel>();

                            commentLookup[comment.news_id].Add(comment);
                        }

                        foreach (var news in newsList)
                        {
                            if (commentLookup.ContainsKey(news.news_id))
                                news.comments = commentLookup[news.news_id];
                        }
                    }
                }
            }

            return Ok(newsList);
        }
    }
}
