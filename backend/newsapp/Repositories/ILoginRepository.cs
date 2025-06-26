using newsapp.Models;

namespace newsapp.Repositories
{
    public interface ILoginRepository
    {
        Task<(bool success, int userId, string message)> SignInAsync(login login);
    }
}
