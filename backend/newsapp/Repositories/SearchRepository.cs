using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly IConfiguration _configuration;

        public SearchRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<TileData>> SearchNewsAsync(string keyword)
        {
            var searchResults = new List<TileData>();
            string connectionString = _configuration.GetConnectionString("NewsDbConnection");

            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT 
                    n.news_id, 
                    n.news_title, 
                    n.contents, 
                    ISNULL(m.image, NULL) AS image, 
                    u.first_name, 
                    u.last_name,
                    n.created_time,
                    u.u_id,
                    ISNULL(likesData.likesCount, 0) AS likes,
                    ISNULL(likesData.unlikesCount, 0) AS unlikes
                FROM NEWS n
                JOIN USERS u ON n.u_id = u.u_id
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                LEFT JOIN (
                    SELECT 
                        news_id,
                        SUM(CASE WHEN likes = 1 THEN 1 ELSE 0 END) AS likesCount,
                        SUM(CASE WHEN unlikes = 1 THEN 1 ELSE 0 END) AS unlikesCount
                    FROM LIKE_UNLIKE
                    GROUP BY news_id
                ) AS likesData ON likesData.news_id = n.news_id
                WHERE n.active = 1 AND 
                      (LOWER(n.news_title) LIKE @keyword OR LOWER(n.contents) LIKE @keyword)
                ORDER BY n.created_time DESC";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@keyword", $"%{keyword.ToLower()}%");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var imageBytes = reader["image"] as byte[];
                string? base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

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

            return searchResults;
        }
    }
}
