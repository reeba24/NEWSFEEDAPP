using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface IFollowingRepository
    {
        Task<IEnumerable<TileData>> GetFollowedNewsAsync(int userId);
    }
}
