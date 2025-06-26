using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface IFollowRepository
    {
        Task<string> ToggleFollowAsync(Follow follow);
    }
}
