using System.Data.SqlClient;
using Dapper;
using NewsApp.Repository.Models;

namespace newsapp.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeedRepository> _logger;

        public FeedRepository(IConfiguration configuration, ILogger<FeedRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetFeedStatusAsync(int userId)
        {
            try
            {
                string connStr = _configuration.GetConnectionString("NewsDbConnection")!;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    string prefQuery = "SELECT COUNT(*) FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                    using (SqlCommand prefCmd = new SqlCommand(prefQuery, conn))
                    {
                        prefCmd.Parameters.AddWithValue("@UserId", userId);
                        int prefCount = (int)await prefCmd.ExecuteScalarAsync();
                        if (prefCount == 0)
                            return "noprefs";
                    }

                    string followQuery = "SELECT COUNT(*) FROM FOLLOWED WHERE followed_by_uid = @UserId AND activeind = 1";
                    using (SqlCommand followCmd = new SqlCommand(followQuery, conn))
                    {
                        followCmd.Parameters.AddWithValue("@UserId", userId);
                        int followCount = (int)await followCmd.ExecuteScalarAsync();

                        if (followCount == 0)
                        {
                            string prefNewsQuery = @"
                                SELECT TOP 1 1 FROM NEWS 
                                WHERE pref_id IN (
                                    SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId
                                ) AND active = 1";

                            using (SqlCommand newsCmd = new SqlCommand(prefNewsQuery, conn))
                            {
                                newsCmd.Parameters.AddWithValue("@UserId", userId);
                                var reader = await newsCmd.ExecuteReaderAsync();
                                if (reader.HasRows)
                                    return "ready";
                                else
                                    return "nonews";
                            }
                        }
                    }

                    string followedNewsQuery = @"
                        SELECT COUNT(*) 
                        FROM NEWS 
                        WHERE u_id IN (
                            SELECT followed_uid 
                            FROM FOLLOWED 
                            WHERE followed_by_uid = @UserId AND activeind = 1
                        ) AND active = 1";

                    using (SqlCommand newsCmd = new SqlCommand(followedNewsQuery, conn))
                    {
                        newsCmd.Parameters.AddWithValue("@UserId", userId);
                        int newsCount = (int)await newsCmd.ExecuteScalarAsync();
                        return newsCount == 0 ? "nonews" : "ready";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feed status for user {UserId}", userId);
                return "error";
            }
        }
    }
}
