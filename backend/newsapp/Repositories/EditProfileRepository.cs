using NewsApp.Repository.Models;
using System.Data.SqlClient;
using Dapper;

namespace newsapp.Repositories
{
    public class EditProfileRepository : IEditProfileRepository
    {
        private readonly string _connectionString;

        public EditProfileRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NewsDbConnection")!;
        }

        public async Task<EditProfile?> GetProfileAsync(int userId)
        {
            var profile = new EditProfile
            {
                UserId = userId,
                PreferenceIds = new List<int>()
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var userQuery = @"SELECT first_name, last_name, about FROM USERS WHERE u_id = @UserId";
                using (var reader = await connection.ExecuteReaderAsync(userQuery, new { UserId = userId }))
                {
                    if (reader.Read())
                    {
                        profile.FirstName = reader["first_name"]?.ToString();
                        profile.LastName = reader["last_name"]?.ToString();
                        profile.About = reader["about"]?.ToString();
                    }
                }

                var prefQuery = @"SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                var prefs = await connection.QueryAsync<int>(prefQuery, new { UserId = userId });
                profile.PreferenceIds = prefs.ToList();
            }

            return profile;
        }

        public async Task<bool> UpdateProfileAsync(EditProfile profile)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var tx = connection.BeginTransaction();

                try
                {
                    var updateUser = @"
                        UPDATE USERS SET first_name = @FirstName, last_name = @LastName, about = @About, modified_time = GETDATE()
                        WHERE u_id = @UserId";

                    await connection.ExecuteAsync(updateUser, new
                    {
                        profile.FirstName,
                        profile.LastName,
                        About = profile.About ?? (object)DBNull.Value,
                        profile.UserId
                    }, tx);

                    await connection.ExecuteAsync("DELETE FROM USER_PREF_BRIDGE WHERE u_id = @UserId", new { profile.UserId }, tx);

                    foreach (var prefId in profile.PreferenceIds)
                    {
                        var insert = @"
                            INSERT INTO USER_PREF_BRIDGE (u_id, pref_id, created_time, modified_time, active)
                            VALUES (@UserId, @PrefId, GETDATE(), GETDATE(), 1)";

                        await connection.ExecuteAsync(insert, new { profile.UserId, PrefId = prefId }, tx);
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
