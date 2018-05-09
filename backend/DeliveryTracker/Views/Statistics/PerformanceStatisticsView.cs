using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Statistics;
using Npgsql;

namespace DeliveryTracker.Views.Statistics
{
    public sealed class PerformanceStatisticsView : IView
    {
        #region sql

        private const string SqlGetManagerPerformance = @"
select
    date,
    created_tasks,
    completed_tasks
from manager_statistics
where user_id = @user_id and date > @date
;
";
        private const string SqlGetPerformerPerformance = @"
select
    date,
    completed_tasks
from performer_statistics
where user_id = @user_id and date > @date
;
";
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string Name { get; } = nameof(PerformanceStatisticsView);
        
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
            var list = new List<TaskStatisticsItem>();
            var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            var hasItem = new bool[8];
            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGetManagerPerformance;
                command.Parameters.Add(new NpgsqlParameter("user_id", authorId));
                command.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date.AddDays(-7)));
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {                        
                        var date = reader.GetDateTime(0);
                        var span = date - weekAgo;
                        hasItem[span.Days] = true;

                        var item = new TaskStatisticsItem
                        {
                            DatePoint = date,
                            Created = reader.GetInt32(1),
                            Completed = reader.GetInt32(2)
                        };

                        list.Add(item);
                    }
                }
            }

            for (var i = 0; i < hasItem.Length; i++)
            {
                if (!hasItem[i])
                {
                    var item = new TaskStatisticsItem
                    {
                        DatePoint = weekAgo.AddDays(i),
                        Completed = 0,
                    };

                    list.Add(item);
                }
            }

            return list.OrderBy(p => p.DatePoint).ToList<IDictionaryObject>();
        }

        private static async Task<List<IDictionaryObject>> ReadPerformer(
            Guid performerId,
            NpgsqlConnectionWrapper oc)
        {
            var list = new List<TaskStatisticsItem>();
            var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            var hasItem = new bool[8];

            using (var command = oc.CreateCommand())
            {
                command.CommandText = SqlGetPerformerPerformance;
                command.Parameters.Add(new NpgsqlParameter("user_id", performerId));
                command.Parameters.Add(new NpgsqlParameter("date", weekAgo));
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var date = reader.GetDateTime(0);
                        var span = date - weekAgo;
                        hasItem[span.Days] = true;
                        
                        var item = new TaskStatisticsItem
                        {
                            DatePoint = date,
                            Completed = reader.GetInt32(1),
                        };

                        list.Add(item);
                    }
                }
            }

            for (var i = 0; i < hasItem.Length; i++)
            {
                if (!hasItem[i])
                {
                    var item = new TaskStatisticsItem
                    {
                        DatePoint = weekAgo.AddDays(i),
                        Completed = 0,
                    };

                    list.Add(item);
                }
            }

            return list.OrderBy(p => p.DatePoint).ToList<IDictionaryObject>();
        } 
        
        #endregion
    }
}