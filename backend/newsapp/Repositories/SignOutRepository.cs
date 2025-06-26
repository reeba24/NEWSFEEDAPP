using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace newsapp.Repositories
{
    public class SignOutRepository : ISignOutRepository
    {
        private readonly IConfiguration _configuration;

        public SignOutRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SignOutUserAsync(int userId)
        {
            string connectionString = _configuration.GetConnectionString("NewsDbConnection");

            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string sql = "UPDATE USERS SET signed_in = 0 WHERE u_id = @u_id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u_id", userId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}
