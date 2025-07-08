using System.Data;
using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;

namespace newsapp.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly IDataManager _dataManager;
        private readonly ILogger<FeedRepository> _logger;

        public FeedRepository(IDataManager dataManager, ILogger<FeedRepository> logger)
        {
            _dataManager = dataManager;
            _logger = logger;
        }

        public async Task<string> GetFeedStatusAsync(int userId)
        {
            try
            {
                using var conn = _dataManager.CreateConnection();
                conn.Open();

                string prefQuery = "SELECT COUNT(*) FROM USER_PREF_BRIDGE WHERE u_id = @UserId";
                int prefCount = conn.ExecuteScalar<int>(prefQuery, new { UserId = userId });
                if (prefCount == 0)
                    return "noprefs";

                string followQuery = "SELECT COUNT(*) FROM FOLLOWED WHERE followed_by_uid = @UserId AND activeind = 1";
                int followCount = conn.ExecuteScalar<int>(followQuery, new { UserId = userId });

                if (followCount == 0)
                {
                    string prefNewsQuery = @"
                        SELECT TOP 1 1 FROM NEWS 
                        WHERE pref_id IN (
                            SELECT pref_id FROM USER_PREF_BRIDGE WHERE u_id = @UserId
                        ) AND active = 1";

                    var result = conn.QueryFirstOrDefault<int?>(prefNewsQuery, new { UserId = userId });
                    return result.HasValue ? "ready" : "nonews";
                }

                string followedNewsQuery = @"
                    SELECT COUNT(*) 
                    FROM NEWS 
                    WHERE u_id IN (
                        SELECT followed_uid 
                        FROM FOLLOWED 
                        WHERE followed_by_uid = @UserId AND activeind = 1
                    ) AND active = 1";

                int newsCount = conn.ExecuteScalar<int>(followedNewsQuery, new { UserId = userId });
                return newsCount == 0 ? "nonews" : "ready";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feed status for user {UserId}", userId);
                return "error";
            }
        }
    }
}
