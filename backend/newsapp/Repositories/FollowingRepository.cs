using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;
using System.Data;

namespace newsapp.Repositories
{
    public class FollowingRepository : IFollowingRepository
    {
        private readonly IDataManager _dataManager;

        public FollowingRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<IEnumerable<TileData>> GetFollowedNewsAsync(int userId)
        {
            var list = new List<TileData>();
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            var sql = @"
                SELECT n.news_id, n.news_title, n.contents, m.image, u.first_name, u.last_name,
                       n.created_time, u.u_id AS author_id, n.read_time
                FROM FOLLOWED f
                JOIN USERS u ON f.followed_uid = u.u_id
                JOIN NEWS n ON u.u_id = n.u_id
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                WHERE f.followed_by_uid = @UserId AND f.activeind = 1 AND n.active = 1
                ORDER BY n.created_time DESC";

            var rows = await conn.QueryAsync(sql, new { UserId = userId });

            foreach (var row in rows)
            {
                string contents = row.contents;
                int wordCount = !string.IsNullOrWhiteSpace(contents)
                    ? contents.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    : 0;
                int calculatedReadTime = wordCount / 100;
                if (calculatedReadTime == 0 && wordCount > 0) calculatedReadTime = 1;

                int readTime = row.read_time != null ? (int)row.read_time : calculatedReadTime;

                var news = new TileData
                {
                    news_id = row.news_id,
                    news_title = row.news_title,
                    contents = contents,
                    image = row.image,
                    imageBase64 = row.image != null ? Convert.ToBase64String(row.image) : null,
                    first_name = row.first_name,
                    last_name = row.last_name,
                    created_time = row.created_time,
                    u_id = row.author_id,
                    read_time = readTime,
                    isFollowed = true,
                    hasLiked = false,
                    hasUnliked = false,
                    hasSaved = false,
                    comments = new List<CommentModel>(),
                    likes = 0,
                    unlikes = 0
                };

                var likeQuery = @"
                    SELECT 
                        ISNULL(SUM(CAST(likes AS INT)), 0) AS totalLikes,
                        ISNULL(SUM(CAST(unlikes AS INT)), 0) AS totalUnlikes,
                        MAX(CAST(CASE WHEN u_id = @uid AND like_modified = 1 THEN likes ELSE 0 END AS INT)) AS hasLiked,
                        MAX(CAST(CASE WHEN u_id = @uid AND like_modified = 1 THEN unlikes ELSE 0 END AS INT)) AS hasUnliked
                    FROM LIKE_UNLIKE WHERE news_id = @nid";

                var likeStats = await conn.QueryFirstAsync(likeQuery, new { uid = userId, nid = news.news_id });
                news.likes = likeStats.totalLikes;
                news.unlikes = likeStats.totalUnlikes;
                news.hasLiked = likeStats.hasLiked == 1;
                news.hasUnliked = likeStats.hasUnliked == 1;

                string saveQuery = "SELECT COUNT(*) FROM SAVED WHERE news_id = @nid AND u_id = @uid AND active = 1";
                int saved = await conn.ExecuteScalarAsync<int>(saveQuery, new { nid = news.news_id, uid = userId });
                news.hasSaved = saved > 0;

                string commentQuery = @"
                    SELECT comment_id, news_id, u_id, comments, created_time 
                    FROM COMMENT WHERE news_id = @nid AND active = 1
                    ORDER BY created_time DESC";

                var commentRows = await conn.QueryAsync<CommentModel>(commentQuery, new { nid = news.news_id });
                news.comments = commentRows.ToList();

                list.Add(news);
            }

            return list;
        }
    }
}
