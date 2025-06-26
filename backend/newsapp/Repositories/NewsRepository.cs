using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly IConfiguration _config;

        public NewsRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<TileData>> GetAllNewsAsync(int userId)
        {
            var newsList = new List<TileData>();
            var followedUids = new HashSet<int>();
            var preferredPrefIds = new HashSet<int>();

            using SqlConnection conn = new SqlConnection(_config.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            using (var cmd = new SqlCommand("SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @uid AND activeind = 1", conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    followedUids.Add((int)reader["followed_uid"]);
            }

            using (var cmd = new SqlCommand("SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @uid", conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    preferredPrefIds.Add((int)reader["pref_id"]);
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

            using (var cmd = new SqlCommand(newsSql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int authorId = (int)reader["u_id"];
                    int prefId = (int)reader["pref_id"];
                    if (!followedUids.Contains(authorId) && !preferredPrefIds.Contains(prefId))
                        continue;

                    byte[] imageBytes = reader["image"] as byte[];
                    newsList.Add(new TileData
                    {
                        news_id = (int)reader["news_id"],
                        news_title = reader["news_title"].ToString(),
                        contents = reader["contents"].ToString(),
                        image = imageBytes,
                        imageBase64 = imageBytes != null ? Convert.ToBase64String(imageBytes) : null,
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

            if (newsList.Any())
            {
                string idList = string.Join(",", newsList.Select(n => n.news_id));
                string commentSql = $@"
                    SELECT comment_id, news_id, u_id, comments, created_time 
                    FROM COMMENT 
                    WHERE active = 1 AND news_id IN ({idList})
                    ORDER BY created_time DESC";

                using (var cmd = new SqlCommand(commentSql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var lookup = newsList.ToDictionary(n => n.news_id);
                    while (await reader.ReadAsync())
                    {
                        int nid = (int)reader["news_id"];
                        if (lookup.ContainsKey(nid))
                        {
                            lookup[nid].comments.Add(new CommentModel
                            {
                                comment_id = (int)reader["comment_id"],
                                news_id = nid,
                                u_id = (int)reader["u_id"],
                                comments = reader["comments"].ToString(),
                                created_time = (DateTime)reader["created_time"]
                            });
                        }
                    }
                }
            }

            return newsList;
        }
    }
}
