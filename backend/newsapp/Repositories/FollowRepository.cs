﻿using Dapper;
using NewsApp.Repository.Models;
using newsapp.Data;
using System.Data;

namespace newsapp.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly IDataManager _dataManager;
        private readonly ILogger<FollowRepository> _logger;

        public FollowRepository(IDataManager dataManager, ILogger<FollowRepository> logger)
        {
            _dataManager = dataManager;
            _logger = logger;
        }

        public async Task<string> ToggleFollowAsync(Follow follow)
        {
            using var conn = _dataManager.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var activeInd = await conn.ExecuteScalarAsync<int?>(@"
                    SELECT activeind FROM FOLLOWED 
                    WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid",
                    follow, transaction);

                bool isFollowed = activeInd.HasValue && activeInd.Value == 1;

                var name = await conn.QueryFirstOrDefaultAsync<(string First, string Last)>(
                    "SELECT first_name, last_name FROM USERS WHERE u_id = @UserId",
                    new { UserId = follow.FollowedByUid }, transaction);

                string notifText;
                int notificationType;

                if (!isFollowed)
                {
                    await conn.ExecuteAsync(@"
                        IF EXISTS (
                            SELECT 1 FROM FOLLOWED WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid
                        )
                            UPDATE FOLLOWED SET activeind = 1 
                            WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid
                        ELSE
                            INSERT INTO FOLLOWED (followed_by_uid, followed_uid, activeind)
                            VALUES (@FollowedByUid, @FollowedUid, 1)", follow, transaction);

                    notifText = $"{name.First} {name.Last} started following you.";
                    notificationType = 3;
                }
                else
                {
                    await conn.ExecuteAsync(@"
                        UPDATE FOLLOWED 
                        SET activeind = 0 
                        WHERE followed_by_uid = @FollowedByUid AND followed_uid = @FollowedUid",
                        follow, transaction);

                    notifText = $"{name.First} {name.Last} unfollowed you.";
                    notificationType = 4;
                }

                await conn.ExecuteAsync(@"
                    INSERT INTO NOTIFICATION (u_id, notificationtype_id, created_time, notification_text, active)
                    VALUES (@ToUid, @TypeId, GETDATE(), @Text, 1)",
                    new { ToUid = follow.FollowedUid, TypeId = notificationType, Text = notifText }, transaction);

                transaction.Commit();

                return isFollowed ? "Unfollowed successfully." : "Followed successfully.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Follow toggle failed.");
                throw;
            }
        }
    }
}
