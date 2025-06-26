namespace newsapp.Repositories
{
    public interface ISignOutRepository
    {
        Task<bool> SignOutUserAsync(int userId);
    }
}
