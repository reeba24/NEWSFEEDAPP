using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SearchController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{keyword}")]
        public IActionResult SearchNews(string keyword)
        {
            var searchResults = new List<TileData>();
            var connectionString = _configuration.GetConnectionString("NewsDbConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT n.news_id, n.news_title, n.contents, m.image, u.first_name, u.last_name,
                           ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                           ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                           n.created_time, u.u_id
                    FROM NEWS n
                    JOIN USERS u ON n.u_id = u.u_id
                    LEFT JOIN MEDIA m ON n.news_id = m.news_id
                    LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                    WHERE n.active = 1 AND 
                          (LOWER(n.news_title) LIKE @keyword OR LOWER(n.contents) LIKE @keyword)
                    GROUP BY n.news_id, n.news_title, n.contents, m.image, u.first_name, u.last_name, n.created_time, u.u_id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword.ToLower() + "%");

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var imageBytes = reader["image"] as byte[];
                    string base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                    searchResults.Add(new TileData
                    {
                        news_id = (int)reader["news_id"],
                        news_title = reader["news_title"].ToString(),
                        contents = reader["contents"].ToString(),
                        image = imageBytes,
                        imageBase64 = base64Image,
                        first_name = reader["first_name"].ToString(),
                        last_name = reader["last_name"].ToString(),
                        likes = (int)reader["likes"],
                        unlikes = (int)reader["unlikes"],
                        created_time = (DateTime)reader["created_time"],
                        u_id = (int)reader["u_id"],
                        comments = new List<CommentModel>()
                    });
                }
                reader.Close();
            }

            return Ok(searchResults);
        }
    }
}