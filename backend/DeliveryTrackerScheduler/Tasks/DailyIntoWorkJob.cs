using System;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Tasks;
using DeliveryTrackerScheduler.Common;
using NLog;
using Npgsql;
using NpgsqlTypes;
using Quartz;

namespace DeliveryTrackerScheduler.Tasks
{
    public sealed class DailyIntoWorkJob : DeliveryTrackerJob
    {
        #region sql

        private static readonly string GetDailyTasks = $@"
select
    id,
    performer_id,
    delivery_from,
    delivery_to
from tasks delivery_from::date = @date
    and state_id = @state_id

limit {Limit}
offset @offset
;
";
        
        #endregion
        
        #region fields

        private const string Limit = "1000";
        
        private readonly IPostgresConnectionProvider cp;

        #endregion
        
        public DailyIntoWorkJob(
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }


        protected override async Task ExecuteInternal(
            IJobExecutionContext context,
            Logger logger)
        {
            using (var conn = this.cp.Create().Connect())
            {
                var recordsAffected = -1;
                var date = DateTime.Now.Date;
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = GetDailyTasks;
                    command.Parameters.Add(new NpgsqlParameter("date", date.Date).WithType(NpgsqlDbType.Date));
                    command.Parameters.Add(new NpgsqlParameter("state_id", DefaultTaskStates.Queue.Id));
                    
                }
                
                while (recordsAffected != 0)
                {
                    
                }
            }
        }
    }
}