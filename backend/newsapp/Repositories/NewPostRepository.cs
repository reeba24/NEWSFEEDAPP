using NewsApp.Repository.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Dapper;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class NewPostRepository : INewPostRepository
    {
        private readonly IDataManager _dataManager;

        public NewPostRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<string> CreatePostAsync(NewsPost post)
        {
            using var con = _dataManager.CreateConnection();
            con.Open(); 
            using var transaction = con.BeginTransaction();

            try
            {
                var nextNewsId = con.ExecuteScalar<int>("SELECT ISNULL(MAX(news_id), 0) + 1 FROM NEWS", transaction: transaction);
                var nextMediaId = con.ExecuteScalar<int>("SELECT ISNULL(MAX(media_id), 0) + 1 FROM MEDIA", transaction: transaction);

                var prefId = con.ExecuteScalar<int?>(
                    "SELECT pref_id FROM PREFERENCES WHERE LTRIM(RTRIM(LOWER(pref_name))) = LTRIM(RTRIM(LOWER(@prefName)))",
                    new { prefName = post.pref_name?.Trim() }, transaction);

                if (prefId == null)
                    throw new Exception("Invalid preference/category name.");

                int readTime = CalculateReadTime(post.contents);

                con.Execute(@"
                    INSERT INTO NEWS (news_id, media_id, pref_id, news_title, contents, u_id, active, created_time, read_time)
                    VALUES (@newsId, @mediaId, @prefId, @title, @content, @uid, @active, @createdTime, @readTime)",
                    new
                    {
                        newsId = nextNewsId,
                        mediaId = nextMediaId,
                        prefId = prefId,
                        title = post.news_title,
                        content = post.contents,
                        uid = post.u_id,
                        active = post.active,
                        createdTime = DateTime.Now,
                        readTime = readTime
                    }, transaction);

                byte[]? imageBytes = null;
                if (post.image != null && post.image.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await post.image.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }

                con.Execute("SET IDENTITY_INSERT MEDIA ON", transaction: transaction);

                con.Execute(@"
                    INSERT INTO MEDIA (media_id, news_id, image)
                    VALUES (@mediaId, @newsId, @image)",
                    new
                    {
                        mediaId = nextMediaId,
                        newsId = nextNewsId,
                        image = (object?)imageBytes ?? DBNull.Value
                    }, transaction);

                con.Execute("SET IDENTITY_INSERT MEDIA OFF", transaction: transaction);

                var author = con.QueryFirstOrDefault<(string FirstName, string LastName)>(
                    "SELECT first_name, last_name FROM USERS WHERE u_id = @uid",
                    new { uid = post.u_id }, transaction);

                var followerIds = con.Query<int>(
                    "SELECT followed_by_uid FROM FOLLOWED WHERE followed_uid = @uid AND activeind = 1",
                    new { uid = post.u_id }, transaction).ToList();

                foreach (var fid in followerIds)
                {
                    con.Execute(@"
                        INSERT INTO NOTIFICATION (u_id, notificationtype_id, created_time, notification_text, active)
                        VALUES (@uid, 2, GETDATE(), @text, 1)",
                        new
                        {
                            uid = fid,
                            text = $"{author.FirstName} {author.LastName} created a new post: {post.news_title}"
                        }, transaction);
                }

                transaction.Commit();
                return "Post created successfully";
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private int CalculateReadTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 1;

            var wordCount = content.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(wordCount / 100.0));
        }
    }
}
