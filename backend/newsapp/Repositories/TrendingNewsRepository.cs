using NewsApp.Repository.Models;
using newsapp.Data;
using System.Data.SqlClient;

namespace newsapp.Repositories
{
    public class TrendingNewsRepository : ITrendingNewsRepository
    {
        private readonly IDataManager _dataManager;

        public TrendingNewsRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<List<TileData>> GetTrendingNewsAsync()
        {
            var trendingNews = new List<TileData>();

            using var conn = (SqlConnection)_dataManager.CreateConnection();
            await conn.OpenAsync();

            string sql = @"
                SELECT 
                    n.news_id, n.news_title, n.contents, m.image,
                    u.u_id, u.first_name, u.last_name,
                    ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                    ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                    n.created_time,
                    n.read_time
                FROM NEWS n
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                LEFT JOIN USERS u ON n.u_id = u.u_id
                LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                WHERE n.active = 1
                GROUP BY 
                    n.news_id, n.news_title, n.contents, m.image,
                    u.u_id, u.first_name, u.last_name, n.created_time, n.read_time
                ORDER BY likes DESC";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                byte[]? imageBytes = reader["image"] as byte[];
                string? base64Image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                string contents = reader["contents"].ToString();
                int wordCount = !string.IsNullOrWhiteSpace(contents)
                    ? contents.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    : 0;

                int calculatedReadTime = wordCount / 100;
                if (calculatedReadTime == 0 && wordCount > 0)
                    calculatedReadTime = 1;

                int readTime = reader["read_time"] != DBNull.Value
                    ? (int)reader["read_time"]
                    : calculatedReadTime;

                trendingNews.Add(new TileData
                {
                    news_id = (int)reader["news_id"],
                    news_title = reader["news_title"].ToString(),
                    contents = contents,
                    image = imageBytes,
                    imageBase64 = base64Image,
                    first_name = reader["first_name"].ToString(),
                    last_name = reader["last_name"].ToString(),
                    u_id = (int)reader["u_id"],
                    likes = (int)reader["likes"],
                    unlikes = (int)reader["unlikes"],
                    created_time = (DateTime)reader["created_time"],
                    read_time = readTime,
                    comments = new List<CommentModel>()
                });
            }

            if (trendingNews.Count > 0)
            {
                string commentSql = $@"
                    SELECT comment_id, news_id, u_id, comments, created_time
                    FROM COMMENT
                    WHERE active = 1 AND news_id IN ({string.Join(",", trendingNews.Select(n => n.news_id))})
                    ORDER BY created_time DESC";

                using var commentCmd = new SqlCommand(commentSql, conn);
                using var commentReader = await commentCmd.ExecuteReaderAsync();

                Dictionary<int, List<CommentModel>> commentLookup = new();

                while (await commentReader.ReadAsync())
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

                foreach (var news in trendingNews)
                {
                    if (commentLookup.ContainsKey(news.news_id))
                        news.comments = commentLookup[news.news_id];
                }
            }

            return trendingNews;
        }
    }
}
