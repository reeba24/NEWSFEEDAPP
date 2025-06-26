using NewsApp.Repository.Models;
namespace newsapp.Repositories
   

{
    public interface IFeedRepository
    {
        Task<string> GetFeedStatusAsync(int userId);
    }
}
