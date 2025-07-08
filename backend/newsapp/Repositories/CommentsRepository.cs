using NewsApp.Repository.Models;
using newsapp.Data;
using Dapper;

namespace newsapp.Repositories
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly IDataManager _dataManager;
        private readonly ILogger<CommentsRepository> _logger;

        public CommentsRepository(IDataManager dataManager, ILogger<CommentsRepository> logger)
        {
            _dataManager = dataManager;
            _logger = logger;
        }

        public async Task<(int commentId, IEnumerable<CommentModel> comments)> AddCommentAsync(NewsComment comment)
        {
            using var conn = (System.Data.SqlClient.SqlConnection)_dataManager.CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                int newCommentId = await conn.ExecuteScalarAsync<int>(
                    "SELECT ISNULL(MAX(comment_id), 0) + 1 FROM COMMENT",
                    transaction: transaction);

                string insertCommentSql = @"
                    INSERT INTO COMMENT (comment_id, news_id, u_id, comments, active, created_time, modified_time)
                    VALUES (@CommentId, @NewsId, @UserId, @Comments, 1, GETDATE(), GETDATE())";

                await conn.ExecuteAsync(insertCommentSql, new
                {
                    CommentId = newCommentId,
                    NewsId = comment.news_id,
                    UserId = comment.u_id,
                    Comments = comment.comments
                }, transaction);

                var user = await conn.QueryFirstOrDefaultAsync<(string FirstName, string LastName)>(
                    "SELECT first_name, last_name FROM USERS WHERE u_id = @UserId",
                    new { UserId = comment.u_id }, transaction);

                var newsInfo = await conn.QueryFirstOrDefaultAsync<(int AuthorId, string Title)>(
                    "SELECT u_id AS AuthorId, news_title AS Title FROM NEWS WHERE news_id = @NewsId",
                    new { NewsId = comment.news_id }, transaction);

                if (newsInfo.AuthorId != 0 && newsInfo.AuthorId != comment.u_id)
                {
                    string notifText = $"{user.FirstName} {user.LastName} commented on your post: {newsInfo.Title}";

                    await conn.ExecuteAsync(@"
                        INSERT INTO NOTIFICATION 
                        (u_id, notificationtype_id, created_time, notification_text, active)
                        VALUES (@UserId, 5, GETDATE(), @Text, 1)",
                        new { UserId = newsInfo.AuthorId, Text = notifText }, transaction);
                }

                var comments = await conn.QueryAsync<CommentModel>(@"
                    SELECT 
                        C.comment_id, C.news_id, C.u_id, C.comments, C.created_time,
                        U.first_name, U.last_name
                    FROM COMMENT C
                    JOIN USERS U ON C.u_id = U.u_id
                    WHERE C.news_id = @NewsId AND C.active = 1
                    ORDER BY C.created_time ASC",
                    new { NewsId = comment.news_id }, transaction);

                transaction.Commit();

                return (newCommentId, comments);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error adding comment for news_id {NewsId}", comment.news_id);
                throw;
            }
        }

        public async Task<IEnumerable<CommentModel>> GetCommentsByNewsIdAsync(int newsId)
        {
            string sql = @"
                SELECT 
                    C.comment_id, C.news_id, C.u_id, C.comments, C.created_time,
                    U.first_name, U.last_name
                FROM COMMENT C
                JOIN USERS U ON C.u_id = U.u_id
                WHERE C.news_id = @NewsId AND C.active = 1
                ORDER BY C.created_time ASC";

            return await _dataManager.QueryAsync<CommentModel>(sql, new { NewsId = newsId });
        }
    }
}
