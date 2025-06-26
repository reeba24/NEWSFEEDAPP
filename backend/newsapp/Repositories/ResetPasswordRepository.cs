using Microsoft.AspNetCore.Identity;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Repositories
{
    public class ResetPasswordRepository : IResetPasswordRepository
    {
        private readonly IConfiguration _configuration;

        public ResetPasswordRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            await con.OpenAsync();

            string checkUserQuery = "SELECT * FROM USERS WHERE email = @Email AND active = 1";
            using SqlCommand checkCmd = new SqlCommand(checkUserQuery, con);
            checkCmd.Parameters.AddWithValue("@Email", email);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                return false; 
            }

            await reader.CloseAsync();

            var hasher = new PasswordHasher<object>();
            string hashedPassword = hasher.HashPassword(null, newPassword);

            string updateQuery = "UPDATE USERS SET password = @Password WHERE email = @Email AND active = 1";
            using SqlCommand updateCmd = new SqlCommand(updateQuery, con);
            updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
            updateCmd.Parameters.AddWithValue("@Email", email);

            int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
