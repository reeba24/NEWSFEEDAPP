using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class SavedRepository : ISavedRepository
    {
        private readonly IDataManager _dataManager;

        public SavedRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<bool> SaveNewsAsync(Saved saved)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string checkQuery = "SELECT COUNT(*) FROM SAVED WHERE u_id = @u_id AND news_id = @news_id";
            int count = await conn.ExecuteScalarAsync<int>(checkQuery, saved);

            if (count > 0)
            {
                string updateQuery = "UPDATE SAVED SET active = 1, modified_time = GETDATE() WHERE u_id = @u_id AND news_id = @news_id";
                await conn.ExecuteAsync(updateQuery, saved);
            }
            else
            {
                string getMaxIdQuery = "SELECT ISNULL(MAX(save_id), 0) + 1 FROM SAVED";
                int newSaveId = await conn.ExecuteScalarAsync<int>(getMaxIdQuery);

                string insertQuery = @"
                    INSERT INTO SAVED (save_id, news_id, u_id, active, created_time, modified_time)
                    VALUES (@save_id, @news_id, @u_id, 1, GETDATE(), GETDATE())";

                await conn.ExecuteAsync(insertQuery, new
                {
                    save_id = newSaveId,
                    saved.news_id,
                    saved.u_id
                });
            }

            return true;
        }

        public async Task<bool> UnsaveNewsAsync(Saved saved)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string updateQuery = @"
                UPDATE SAVED 
                SET active = 0, modified_time = GETDATE()
                WHERE u_id = @u_id AND news_id = @news_id";

            int rowsAffected = await conn.ExecuteAsync(updateQuery, saved);
            return rowsAffected > 0;
        }

        public async Task<List<TileData>> GetSavedNewsAsync(int u_id)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            var followedAuthorIds = new HashSet<int>();
            var followQuery = "SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @u_id";
            var followedRows = await conn.QueryAsync<int>(followQuery, new { u_id });

            foreach (var id in followedRows)
                followedAuthorIds.Add(id);

            string sql = @"
                SELECT n.news_id, n.news_title, n.contents, m.image,
                       u.first_name, u.last_name, u.u_id,
                       ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                       ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                       n.created_time,
                       n.read_time
                FROM SAVED s
                JOIN NEWS n ON s.news_id = n.news_id
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                JOIN USERS u ON n.u_id = u.u_id
                LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                WHERE s.u_id = @u_id AND s.active = 1 AND n.active = 1
                GROUP BY n.news_id, n.news_title, n.contents, m.image, 
                         u.first_name, u.last_name, u.u_id, n.created_time, n.read_time";

            var savedNews = (await conn.QueryAsync(sql, new { u_id })).Select(row =>
            {
                string contents = row.contents;
                int wordCount = !string.IsNullOrWhiteSpace(contents) ? contents.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length : 0;
                int calculatedReadTime = wordCount / 100;
                if (calculatedReadTime == 0 && wordCount > 0) calculatedReadTime = 1;

                int readTime = row.read_time != null ? (int)row.read_time : calculatedReadTime;

                return new TileData
                {
                    news_id = row.news_id,
                    news_title = row.news_title,
                    contents = contents,
                    image = row.image,
                    imageBase64 = row.image != null ? Convert.ToBase64String(row.image) : null,
                    first_name = row.first_name,
                    last_name = row.last_name,
                    u_id = row.u_id,
                    likes = row.likes,
                    unlikes = row.unlikes,
                    created_time = row.created_time,
                    read_time = readTime,
                    isFollowed = followedAuthorIds.Contains((int)row.u_id),
                    comments = new List<CommentModel>()
                };
            }).ToList();

            if (!savedNews.Any())
                return savedNews;

            string commentSql = $@"
                SELECT comment_id, news_id, u_id, comments, created_time 
                FROM COMMENT 
                WHERE active = 1 AND news_id IN ({string.Join(",", savedNews.Select(n => n.news_id))})
                ORDER BY created_time DESC";

            var commentLookup = new Dictionary<int, List<CommentModel>>();
            var commentRows = await conn.QueryAsync<CommentModel>(commentSql);

            foreach (var comment in commentRows)
            {
                if (!commentLookup.ContainsKey(comment.news_id))
                    commentLookup[comment.news_id] = new List<CommentModel>();

                commentLookup[comment.news_id].Add(comment);
            }

            foreach (var news in savedNews)
            {
                if (commentLookup.ContainsKey(news.news_id))
                    news.comments = commentLookup[news.news_id];
            }

            return savedNews;
        }
    }
}
