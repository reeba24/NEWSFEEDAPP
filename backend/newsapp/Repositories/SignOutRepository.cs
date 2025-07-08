using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using newsapp.Data;


namespace newsapp.Repositories
{
    public class SignOutRepository : ISignOutRepository
    {
        private readonly IDataManager _dataManager;

        public SignOutRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<bool> SignOutUserAsync(int userId)
        {
            using var conn = (SqlConnection)_dataManager.CreateConnection();
            await conn.OpenAsync();

            string sql = "UPDATE USERS SET signed_in = 0 WHERE u_id = @u_id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u_id", userId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}
