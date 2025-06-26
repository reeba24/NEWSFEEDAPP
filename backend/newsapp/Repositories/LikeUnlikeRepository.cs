using Dapper;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Repositories
{
    public class LikeUnlikeRepository : ILikeUnlikeRepository
    {
        private readonly IConfiguration _configuration;

        public LikeUnlikeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> PerformLikeActionAsync(Like model)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            var checkQuery = "SELECT like_modified FROM LIKE_UNLIKE WHERE u_id = @u_id AND news_id = @news_id";
            var likeModifiedObj = await conn.ExecuteScalarAsync<object>(checkQuery, new { model.u_id, model.news_id });

            if (likeModifiedObj != null)
            {
                bool likeModified = Convert.ToBoolean(likeModifiedObj);
                string updateQuery = "";

                if (model.action == "like")
                {
                    updateQuery = @"
                        UPDATE LIKE_UNLIKE 
                        SET likes = 1, unlikes = 0, like_modified = 1, modified_time = GETDATE() 
                        WHERE u_id = @u_id AND news_id = @news_id";
                }
                else if (model.action == "unlike")
                {
                    updateQuery = likeModified
                        ? @"UPDATE LIKE_UNLIKE SET likes = 0, unlikes = 1, like_modified = 0, modified_time = GETDATE() 
                            WHERE u_id = @u_id AND news_id = @news_id"
                        : @"UPDATE LIKE_UNLIKE SET unlikes = 1, modified_time = GETDATE() 
                            WHERE u_id = @u_id AND news_id = @news_id";
                }

                await conn.ExecuteAsync(updateQuery, new { model.u_id, model.news_id });
            }
            else
            {
                string getMaxIdQuery = "SELECT ISNULL(MAX(like_id), 0) + 1 FROM LIKE_UNLIKE";
                int newLikeId = await conn.ExecuteScalarAsync<int>(getMaxIdQuery);

                int likes = model.action == "like" ? 1 : 0;
                int unlikes = model.action == "unlike" ? 1 : 0;
                int likeModified = model.action == "like" ? 1 : 0;

                string insertQuery = @"
                    INSERT INTO LIKE_UNLIKE (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) 
                    VALUES (@like_id, @news_id, @u_id, @likes, @unlikes, @like_modified, GETDATE())";

                await conn.ExecuteAsync(insertQuery, new
                {
                    like_id = newLikeId,
                    model.news_id,
                    model.u_id,
                    likes,
                    unlikes,
                    like_modified = likeModified
                });
            }

            return $"{model.action} successful";
        }

        public async Task<(bool hasLiked, bool hasUnliked, bool hasSaved)> GetFullStatusAsync(int newsId, int userId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await conn.OpenAsync();

            bool hasLiked = false, hasUnliked = false, hasSaved = false;

            var likeStatus = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT like_modified, likes, unlikes FROM LIKE_UNLIKE WHERE news_id = @newsId AND u_id = @userId",
                new { newsId, userId });

            if (likeStatus != null)
            {
                bool likeModified = likeStatus.like_modified != null && Convert.ToBoolean(likeStatus.like_modified);
                int likes = likeStatus.likes != null ? Convert.ToInt32(likeStatus.likes) : 0;
                int unlikes = likeStatus.unlikes != null ? Convert.ToInt32(likeStatus.unlikes) : 0;

                hasLiked = likeModified && likes > 0;
                hasUnliked = !likeModified && unlikes > 0;
            }

            var saved = await conn.ExecuteScalarAsync<object>(
                "SELECT 1 FROM SAVED WHERE news_id = @newsId AND u_id = @userId AND active = 1",
                new { newsId, userId });

            hasSaved = saved != null;

            return (hasLiked, hasUnliked, hasSaved);
        }
    }
}
