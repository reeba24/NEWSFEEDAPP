using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface ICommentsRepository
    {
        Task<(int commentId, IEnumerable<CommentModel> comments)> AddCommentAsync(NewsComment comment);
        Task<IEnumerable<CommentModel>> GetCommentsByNewsIdAsync(int newsId);
    }
}
