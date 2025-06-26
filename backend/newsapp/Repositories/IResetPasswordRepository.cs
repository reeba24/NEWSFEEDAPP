namespace newsapp.Repositories
{
    public interface IResetPasswordRepository
    {
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}
