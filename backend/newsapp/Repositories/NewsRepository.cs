using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;
using System.Data;

namespace newsapp.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly IDataManager _dataManager;

        public NewsRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<IEnumerable<TileData>> GetAllNewsAsync(int userId)
        {
            var newsList = new List<TileData>();
            var followedUids = new HashSet<int>();
            var preferredPrefIds = new HashSet<int>();

            using var conn = _dataManager.CreateConnection();
            conn.Open(); 
            using var transaction = conn.BeginTransaction();

            var followed = await conn.QueryAsync<int>(
                "SELECT followed_uid FROM FOLLOWED WHERE followed_by_uid = @uid AND activeind = 1",
                new { uid = userId }, transaction);
            foreach (var id in followed)
                followedUids.Add(id);

            var prefs = await conn.QueryAsync<int>(
                "SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @uid",
                new { uid = userId }, transaction);
            foreach (var pid in prefs)
                preferredPrefIds.Add(pid);

            string newsSql = @"
                SELECT 
                    n.news_id, n.news_title, n.contents, m.image,
                    u.u_id, u.first_name, u.last_name,
                    ISNULL(SUM(CAST(lu.likes AS INT)), 0) AS likes,
                    ISNULL(SUM(CAST(lu.unlikes AS INT)), 0) AS unlikes,
                    n.created_time, n.pref_id,
                    ISNULL(n.read_time, 1) AS read_time
                FROM NEWS n
                LEFT JOIN MEDIA m ON n.news_id = m.news_id
                LEFT JOIN USERS u ON n.u_id = u.u_id
                LEFT JOIN LIKE_UNLIKE lu ON n.news_id = lu.news_id
                WHERE n.active = 1
                GROUP BY 
                    n.news_id, n.news_title, n.contents, m.image,
                    u.u_id, u.first_name, u.last_name,
                    n.created_time, n.pref_id, n.read_time";

            var newsItems = await conn.QueryAsync(newsSql, transaction: transaction);

            foreach (var row in newsItems)
            {
                int authorId = row.u_id;
                int prefId = row.pref_id;

                if (authorId != userId && !followedUids.Contains(authorId) && !preferredPrefIds.Contains(prefId))
                    continue;

                byte[] imageBytes = row.image as byte[];

                newsList.Add(new TileData
                {
                    news_id = row.news_id,
                    news_title = row.news_title,
                    contents = row.contents,
                    image = imageBytes,
                    imageBase64 = imageBytes != null ? Convert.ToBase64String(imageBytes) : null,
                    first_name = row.first_name,
                    last_name = row.last_name,
                    u_id = authorId,
                    likes = row.likes,
                    unlikes = row.unlikes,
                    created_time = row.created_time,
                    read_time = row.read_time ?? 1,
                    comments = new List<CommentModel>(),
                    isFollowed = followedUids.Contains(authorId)
                });
            }

            if (!newsList.Any())
            {
                transaction.Commit();
                return newsList;
            }

            string idList = string.Join(",", newsList.Select(n => n.news_id));
            string commentSql = $@"
                SELECT comment_id, news_id, u_id, comments, created_time 
                FROM COMMENT 
                WHERE active = 1 AND news_id IN ({idList})
                ORDER BY created_time DESC";

            var comments = await conn.QueryAsync(commentSql, transaction: transaction);

            var lookup = newsList.ToDictionary(n => n.news_id);
            foreach (var row in comments)
            {
                int nid = row.news_id;
                if (lookup.ContainsKey(nid))
                {
                    lookup[nid].comments.Add(new CommentModel
                    {
                        comment_id = row.comment_id,
                        news_id = nid,
                        u_id = row.u_id,
                        comments = row.comments,
                        created_time = row.created_time
                    });
                }
            }

            transaction.Commit();
            return newsList;
        }
    }
}
