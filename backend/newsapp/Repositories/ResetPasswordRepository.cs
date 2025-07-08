using Microsoft.AspNetCore.Identity;
using NewsApp.Repository.Models;
using newsapp.Data;
using Dapper;

namespace newsapp.Repositories
{
    public class ResetPasswordRepository : IResetPasswordRepository
    {
        private readonly IDataManager _dataManager;

        public ResetPasswordRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();

            string checkUserQuery = "SELECT COUNT(*) FROM USERS WHERE email = @Email AND active = 1";
            int userExists = await conn.ExecuteScalarAsync<int>(checkUserQuery, new { Email = email });

            if (userExists == 0)
                return false;

            var hasher = new PasswordHasher<object>();
            string hashedPassword = hasher.HashPassword(null, newPassword);

            string updateQuery = "UPDATE USERS SET password = @Password WHERE email = @Email AND active = 1";
            int rowsAffected = await conn.ExecuteAsync(updateQuery, new { Password = hashedPassword, Email = email });

            return rowsAffected > 0;
        }
    }
}
