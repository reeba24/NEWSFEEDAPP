using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

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
        public IActionResult GetAllNews()
        {
            var newsList = new List<TileData>();

            string connStr = _configuration.GetConnectionString("NewsDbConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        n.news_id, 
                        n.news_title, 
                        n.contents, 
                        m.image,
                        u.first_name, 
                        u.last_name,
                        ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                        ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                        n.created_time
                    FROM NEWS n
                    LEFT JOIN MEDIA m ON n.news_id = m.news_id
                    LEFT JOIN USERS u ON n.u_id = u.u_id
                    LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                    WHERE n.active = 1
                    GROUP BY 
                        n.news_id, 
                        n.news_title, 
                        n.contents, 
                        m.image,
                        u.first_name, 
                        u.last_name,
                        n.created_time";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var imageBytes = reader["image"] as byte[];
                        string imageBase64 = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                        var tile = new TileData
                        {
                            news_id = (int)reader["news_id"],
                            news_title = reader["news_title"].ToString(),
                            contents = reader["contents"].ToString(),
                            image = imageBytes,
                            imageBase64 = imageBase64,
                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            likes = (int)reader["likes"],
                            unlikes = (int)reader["unlikes"],
                            created_time = (DateTime)reader["created_time"]
                        };

                        newsList.Add(tile);
                    }
                }
            }

            return Ok(newsList);
        }
    }
}
