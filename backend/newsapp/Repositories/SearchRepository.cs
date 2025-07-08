using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly IDataManager _dataManager;

        public SearchRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<List<TileData>> SearchNewsAsync(string keyword)
        {
            var searchResults = new List<TileData>();
            using var conn = _dataManager.CreateConnection();
            conn.Open();

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

            var results = await conn.QueryAsync(sql, new { keyword = $"%{keyword.ToLower()}%" });

            foreach (var row in results)
            {
                string contents = row.contents;
                int wordCount = !string.IsNullOrWhiteSpace(contents)
                    ? contents.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    : 0;
                int calculatedReadTime = wordCount / 100;
                if (calculatedReadTime == 0 && wordCount > 0) calculatedReadTime = 1;

                byte[]? imageBytes = row.image;
                string? base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                searchResults.Add(new TileData
                {
                    news_id = row.news_id,
                    news_title = row.news_title,
                    contents = contents,
                    image = imageBytes,
                    imageBase64 = base64Image,
                    first_name = row.first_name,
                    last_name = row.last_name,
                    likes = row.likes,
                    unlikes = row.unlikes,
                    created_time = row.created_time,
                    u_id = row.u_id,
                    read_time = calculatedReadTime,
                    comments = new List<CommentModel>()
                });
            }

            return searchResults;
        }
    }
}
