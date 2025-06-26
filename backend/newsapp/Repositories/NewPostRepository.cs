using NewsApp.Repository.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;
using Dapper;

namespace newsapp.Repositories
{
    public class NewPostRepository : INewPostRepository
    {
        private readonly IConfiguration _configuration;

        public NewPostRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreatePostAsync(NewsPost post)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await con.OpenAsync();

            var nextNewsId = await con.ExecuteScalarAsync<int>("SELECT ISNULL(MAX(news_id), 0) + 1 FROM NEWS");
            var nextMediaId = await con.ExecuteScalarAsync<int>("SELECT ISNULL(MAX(media_id), 0) + 1 FROM MEDIA");

            var prefId = await con.ExecuteScalarAsync<int?>(
                "SELECT pref_id FROM PREFERENCES WHERE LTRIM(RTRIM(LOWER(pref_name))) = LTRIM(RTRIM(LOWER(@prefName)))",
                new { prefName = post.pref_name?.Trim() });

            if (prefId == null)
                throw new Exception("Invalid preference/category name.");

            await con.ExecuteAsync(@"
                INSERT INTO NEWS (news_id, media_id, pref_id, news_title, contents, u_id, active, created_time)
                VALUES (@newsId, @mediaId, @prefId, @title, @content, @uid, @active, @createdTime)",
                new
                {
                    newsId = nextNewsId,
                    mediaId = nextMediaId,
                    prefId = prefId,
                    title = post.news_title,
                    content = post.contents,
                    uid = post.u_id,
                    active = post.active,
                    createdTime = DateTime.Now
                });

            byte[] imageBytes = null;
            if (post.image != null && post.image.Length > 0)
            {
                using var ms = new MemoryStream();
                await post.image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            await con.ExecuteAsync("SET IDENTITY_INSERT MEDIA ON");

            await con.ExecuteAsync(@"
                INSERT INTO MEDIA (media_id, news_id, image)
                VALUES (@mediaId, @newsId, @image)",
                new
                {
                    mediaId = nextMediaId,
                    newsId = nextNewsId,
                    image = (object?)imageBytes ?? DBNull.Value
                });

            await con.ExecuteAsync("SET IDENTITY_INSERT MEDIA OFF");

            var author = await con.QueryFirstOrDefaultAsync<(string FirstName, string LastName)>(
                "SELECT first_name, last_name FROM USERS WHERE u_id = @uid", new { uid = post.u_id });

            var followerIds = (await con.QueryAsync<int>(
                "SELECT followed_by_uid FROM FOLLOWED WHERE followed_uid = @uid", new { uid = post.u_id })).ToList();

            if (followerIds.Any())
            {
                foreach (var fid in followerIds)
                {
                    await con.ExecuteAsync(@"
                        INSERT INTO NOTIFICATION (u_id, notificationtype_id, created_time, notification_text, active)
                        VALUES (@uid, 2, GETDATE(), @text, 1)",
                        new
                        {
                            uid = fid,
                            text = $"{author.FirstName} {author.LastName} created a new post: {post.news_title}"
                        });
                }
            }

            return "Post created successfully";
        }
    }
}
