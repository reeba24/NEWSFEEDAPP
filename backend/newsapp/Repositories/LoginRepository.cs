using Microsoft.AspNetCore.Identity;
using newsapp.Models;
using System.Data.SqlClient;
using Dapper;

namespace newsapp.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IConfiguration _configuration;

        public LoginRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool success, int userId, string message)> SignInAsync(login login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.password))
                return (false, 0, "Email and password are required.");

            string normalizedEmail = login.email.Trim().ToLower();

            using var con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection"));
            string query = "SELECT u_id, password FROM USERS WHERE email = @Email AND active = 1";
            var user = await con.QueryFirstOrDefaultAsync<(int u_id, string password)>(query, new { Email = normalizedEmail });

            if (user.u_id == 0)
                return (false, 0, "User not found or inactive.");

            var hasher = new PasswordHasher<login>();
            var result = hasher.VerifyHashedPassword(login, user.password, login.password);

            if (result == PasswordVerificationResult.Success)
                return (true, user.u_id, "Login successful");

            return (false, 0, "Invalid password.");
        }
    }
}
