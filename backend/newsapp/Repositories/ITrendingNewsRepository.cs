using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface ITrendingNewsRepository
    {
        Task<List<TileData>> GetTrendingNewsAsync();
    }
}
