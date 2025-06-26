using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface INewPostRepository
    {
        Task<string> CreatePostAsync(NewsPost post);
    }
}
