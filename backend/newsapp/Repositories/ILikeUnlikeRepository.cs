using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface ILikeUnlikeRepository
    {
        Task<string> PerformLikeActionAsync(Like model);
        Task<(bool hasLiked, bool hasUnliked, bool hasSaved)> GetFullStatusAsync(int newsId, int userId);
    }
}
