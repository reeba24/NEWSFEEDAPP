using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Repositories
{
    public class SavedRepository : ISavedRepository
    {
        private readonly IConfiguration _configuration;

        public SavedRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SaveNewsAsync(Saved saved)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            string checkQuery = "SELECT COUNT(*) FROM SAVED WHERE u_id = @u_id AND news_id = @news_id";
            SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@u_id", saved.u_id);
            checkCmd.Parameters.AddWithValue("@news_id", saved.news_id);

            int count = (int)await checkCmd.ExecuteScalarAsync();

            if (count > 0)
            {
                string updateQuery = "UPDATE SAVED SET active = 1, modified_time = GETDATE() WHERE u_id = @u_id AND news_id = @news_id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@u_id", saved.u_id);
                updateCmd.Parameters.AddWithValue("@news_id", saved.news_id);
                await updateCmd.ExecuteNonQueryAsync();
            }
            else
            {
                string getMaxIdQuery = "SELECT ISNULL(MAX(save_id), 0) + 1 FROM SAVED";
                SqlCommand maxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                int newSaveId = (int)await maxIdCmd.ExecuteScalarAsync();

                string insertQuery = @"
                    INSERT INTO SAVED (save_id, news_id, u_id, active, created_time, modified_time)
                    VALUES (@save_id, @news_id, @u_id, 1, GETDATE(), GETDATE())";

                SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@save_id", newSaveId);
                insertCmd.Parameters.AddWithValue("@u_id", saved.u_id);
                insertCmd.Parameters.AddWithValue("@news_id", saved.news_id);
                await insertCmd.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<bool> UnsaveNewsAsync(Saved saved)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            string updateQuery = @"
                UPDATE SAVED 
                SET active = 0, modified_time = GETDATE()
                WHERE u_id = @u_id AND news_id = @news_id";

            SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@u_id", saved.u_id);
            updateCmd.Parameters.AddWithValue("@news_id", saved.news_id);

            int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<TileData>> GetSavedNewsAsync(int u_id)
        {
            var savedNewsList = new List<TileData>();
            var followedAuthorIds = new HashSet<int>();
            string connStr = _configuration.GetConnectionString("NewsDbConnection");

            using SqlConnection conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using (SqlCommand followCmd = new SqlCommand("SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @u_id", conn))
            {
                followCmd.Parameters.AddWithValue("@u_id", u_id);
                using var reader = await followCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    followedAuthorIds.Add((int)reader["followed_uid"]);
                }
            }

            string sql = @"
                SELECT n.news_id, n.news_title, n.contents, m.image,
                       u.first_name, u.last_name, u.u_id,
                       ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                       ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                       n.created_time
                FROM SAVED s
                JOIN NEWS n ON s.news_id = n.news_id
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                JOIN USERS u ON n.u_id = u.u_id
                LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                WHERE s.u_id = @u_id AND s.active = 1 AND n.active = 1
                GROUP BY n.news_id, n.news_title, n.contents, m.image, 
                         u.first_name, u.last_name, u.u_id, n.created_time";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u_id", u_id);
            using var newsReader = await cmd.ExecuteReaderAsync();

            while (await newsReader.ReadAsync())
            {
                int authorId = (int)newsReader["u_id"];
                var imageBytes = newsReader["image"] as byte[];
                string imageBase64 = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

                savedNewsList.Add(new TileData
                {
                    news_id = (int)newsReader["news_id"],
                    news_title = newsReader["news_title"].ToString(),
                    contents = newsReader["contents"].ToString(),
                    image = imageBytes,
                    imageBase64 = imageBase64,
                    first_name = newsReader["first_name"].ToString(),
                    last_name = newsReader["last_name"].ToString(),
                    u_id = authorId,
                    likes = (int)newsReader["likes"],
                    unlikes = (int)newsReader["unlikes"],
                    created_time = (DateTime)newsReader["created_time"],
                    isFollowed = followedAuthorIds.Contains(authorId),
                    comments = new List<CommentModel>()
                });
            }

            if (savedNewsList.Count == 0) return savedNewsList;

            string commentSql = $@"
                SELECT comment_id, news_id, u_id, comments, created_time 
                FROM COMMENT 
                WHERE active = 1 AND news_id IN ({string.Join(",", savedNewsList.Select(n => n.news_id))})
                ORDER BY created_time DESC";

            using SqlCommand commentCmd = new SqlCommand(commentSql, conn);
            using var commentReader = await commentCmd.ExecuteReaderAsync();
            var commentLookup = new Dictionary<int, List<CommentModel>>();

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

            foreach (var news in savedNewsList)
            {
                if (commentLookup.ContainsKey(news.news_id))
                    news.comments = commentLookup[news.news_id];
            }

            return savedNewsList;
        }
    }
}
