using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class EditProfileRepository : IEditProfileRepository
    {
        private readonly IDataManager _dataManager;

        public EditProfileRepository(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<EditProfile?> GetProfileAsync(int userId)
        {
            var profile = new EditProfile
            {
                UserId = userId,
                PreferenceIds = new List<int>()
            };

            using var connection = _dataManager.CreateConnection();
            connection.Open();

            var userQuery = @"SELECT first_name, last_name, about FROM USERS WHERE u_id = @UserId";
            var user = await connection.QueryFirstOrDefaultAsync(userQuery, new { UserId = userId });

            if (user != null)
            {
                profile.FirstName = user.first_name;
                profile.LastName = user.last_name;
                profile.About = user.about;
            }

            var prefQuery = @"SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
            var prefs = await connection.QueryAsync<int>(prefQuery, new { UserId = userId });
            profile.PreferenceIds = prefs.ToList();

            return profile;
        }

        public async Task<bool> UpdateProfileAsync(EditProfile profile)
        {
            using var connection = _dataManager.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var updateUser = @"
                    UPDATE USERS 
                    SET first_name = @FirstName, last_name = @LastName, about = @About, modified_time = GETDATE()
                    WHERE u_id = @UserId";

                await connection.ExecuteAsync(updateUser, new
                {
                    profile.FirstName,
                    profile.LastName,
                    About = profile.About ?? (object)DBNull.Value,
                    profile.UserId
                }, transaction);

                await connection.ExecuteAsync("DELETE FROM USER_PREF_BRIDGE WHERE u_id = @UserId",
                    new { profile.UserId }, transaction);

                string insertPref = @"
                    INSERT INTO USER_PREF_BRIDGE (u_id, pref_id, created_time, modified_time, active)
                    VALUES (@UserId, @PrefId, GETDATE(), GETDATE(), 1)";

                foreach (var prefId in profile.PreferenceIds)
                {
                    await connection.ExecuteAsync(insertPref, new { profile.UserId, PrefId = prefId }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
