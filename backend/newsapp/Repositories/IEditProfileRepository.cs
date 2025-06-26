using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public interface IEditProfileRepository
    {
        Task<EditProfile?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(EditProfile profile);
    }
}
