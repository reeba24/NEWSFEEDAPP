using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface ISavedRepository
    {
        Task<bool> SaveNewsAsync(Saved saved);
        Task<bool> UnsaveNewsAsync(Saved saved);
        Task<List<TileData>> GetSavedNewsAsync(int u_id);
    }
}
