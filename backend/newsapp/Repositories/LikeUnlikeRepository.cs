using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class LikeUnlikeRepository : ILikeUnlikeRepository
    {
        private readonly IDataManager _dataManager;

        public LikeUnlikeRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<string> PerformLikeActionAsync(Like model)
        {
            var checkQuery = "SELECT like_modified FROM LIKE_UNLIKE WHERE u_id = @u_id AND news_id = @news_id";
            var likeModifiedObj = await _dataManager.ExecuteScalarAsync<object>(checkQuery, new { model.u_id, model.news_id });

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
                        ? @"UPDATE LIKE_UNLIKE 
                            SET likes = 0, unlikes = 1, like_modified = 0, modified_time = GETDATE() 
                            WHERE u_id = @u_id AND news_id = @news_id"
                        : @"UPDATE LIKE_UNLIKE 
                            SET unlikes = 1, modified_time = GETDATE() 
                            WHERE u_id = @u_id AND news_id = @news_id";
                }

                await _dataManager.ExecuteAsync(updateQuery, new { model.u_id, model.news_id });
            }
            else
            {
                string getMaxIdQuery = "SELECT ISNULL(MAX(like_id), 0) + 1 FROM LIKE_UNLIKE";
                int newLikeId = await _dataManager.ExecuteScalarAsync<int>(getMaxIdQuery);

                int likes = model.action == "like" ? 1 : 0;
                int unlikes = model.action == "unlike" ? 1 : 0;
                int likeModified = model.action == "like" ? 1 : 0;

                string insertQuery = @"
                    INSERT INTO LIKE_UNLIKE 
                    (like_id, news_id, u_id, likes, unlikes, like_modified, modified_time) 
                    VALUES (@like_id, @news_id, @u_id, @likes, @unlikes, @like_modified, GETDATE())";

                await _dataManager.ExecuteAsync(insertQuery, new
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
            bool hasLiked = false, hasUnliked = false, hasSaved = false;

            var likeStatus = await _dataManager.QueryFirstOrDefaultAsync<dynamic>(
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

            var saved = await _dataManager.ExecuteScalarAsync<object>(
                "SELECT 1 FROM SAVED WHERE news_id = @newsId AND u_id = @userId AND active = 1",
                new { newsId, userId });

            hasSaved = saved != null;

            return (hasLiked, hasUnliked, hasSaved);
        }
    }
}
