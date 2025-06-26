using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface ISearchRepository
    {
        Task<List<TileData>> SearchNewsAsync(string keyword);
    }
}
