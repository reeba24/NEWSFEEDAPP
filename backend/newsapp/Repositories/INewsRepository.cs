using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface INewsRepository
    {
        Task<IEnumerable<TileData>> GetAllNewsAsync(int userId);
    }
}
