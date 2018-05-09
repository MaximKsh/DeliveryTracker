using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Statistics;
using DeliveryTracker.Tasks;
using Npgsql;

namespace DeliveryTracker.Views.Statistics
{
    public sealed class ActualTasksStateDistributionView: IView
    {
        #region sql

        private const string SqlGetForManagers = @"
select
    state_id
from tasks
where author_id = @user_id and state_changed_last_time > @date
;
";
        private const string SqlGetForPerformers = @"
select
    state_id
from tasks
where performer_id = @user_id and state_changed_last_time > @date
;
";
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(ActualTasksStateDistributionView);
        
        /// <inheritdoc />
        public IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole,
            DefaultRoles.PerformerRole
        }.AsReadOnly();
        
        public async Task<ServiceResult<ViewDigest>> GetViewDigestAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            return await Task.FromResult(new ServiceResult<ViewDigest>());
        }

        public async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            if (parameters.TryGetValue("author_id", out var authorIdStr)
                && authorIdStr.Count > 0
                && Guid.TryParse(authorIdStr[0], out var authorId))
            {
                return new ServiceResult<IList<IDictionaryObject>>(await ReadManager(authorId, oc));
            }
            if (parameters.TryGetValue("performer_id", out var performerIdStr)
                 && performerIdStr.Count > 0
                 && Guid.TryParse(performerIdStr[0], out var performerId))
            {
                return new ServiceResult<IList<IDictionaryObject>>(await ReadPerformer(performerId, oc));
            }

            return new ServiceResult<IList<IDictionaryObject>>(new List<IDictionaryObject>());
        }

        public async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            return await Task.FromResult(new ServiceResult<long>(-1));
        }
        
        #endregion
        
        #region private

        private static async Task<List<IDictionaryObject>> ReadManager(
            Guid authorId,
            NpgsqlConnectionWrapper oc)
        {
            var item = new TaskStatisticsItem();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGetForManagers;
                command.Parameters.Add(new NpgsqlParameter("user_id", authorId));
                command.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date.AddDays(-7)));
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var stateId = reader.GetGuid(0);
                        Increment(item, stateId);
                    }
                }
            }

            return new List<IDictionaryObject> { item };
        }

        private static async Task<List<IDictionaryObject>> ReadPerformer(
            Guid performerId,
            NpgsqlConnectionWrapper oc)
        {
            var item = new TaskStatisticsItem();
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGetForPerformers;
                command.Parameters.Add(new NpgsqlParameter("user_id", performerId));
                command.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date.AddDays(-7)));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var stateId = reader.GetGuid(0);
                        Increment(item, stateId);
                    }
                }
            }

            return new List<IDictionaryObject> { item };
        }

        private static void Increment(
            TaskStatisticsItem item,
            Guid state)
        {
            if (state == DefaultTaskStates.Preparing.Id)
            {
                item.Preparing++;
            }
            else if (state == DefaultTaskStates.Queue.Id)
            {
                item.Queue++;
            }
            else if (state == DefaultTaskStates.Waiting.Id)
            {
                item.Waiting++;
            }
            else if (state == DefaultTaskStates.IntoWork.Id)
            {
                item.IntoWork++;
            }
            else if (state == DefaultTaskStates.Delivered.Id)
            {
                item.Delivered++;
            }
            else if (state == DefaultTaskStates.Complete.Id)
            {
                item.Complete++;
            }
            else if (state == DefaultTaskStates.Revoked.Id)
            {
                item.Revoked++;
            }
        }
        
        #endregion
    }
}