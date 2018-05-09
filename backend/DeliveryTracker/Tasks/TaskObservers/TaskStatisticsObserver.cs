using System;
using System.Threading.Tasks;
using Npgsql;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public sealed class TaskStatisticsObserver : TaskObserverBase
    {
        #region sql 

        private const string SqlAddCreatedTaskManager = @"
insert into manager_statistics (id, user_id, date, created_tasks, completed_tasks)
values (@id, @user_id, @date, @created_tasks, @completed_tasks)
on conflict (user_id, date) do update set created_tasks = manager_statistics.created_tasks + EXCLUDED.created_tasks
;
";

        private const string SqlAddCompletedTaskManager = @"
update manager_statistics 
set completed_tasks = completed_tasks + @completed_tasks
where user_id = @user_id and date = @date
;
";
        
        private const string SqlAddCompletedTaskPerformer = @"
insert into performer_statistics (id, user_id, date, completed_tasks)
values (@id, @user_id, @date, @completed_tasks)
on conflict (user_id, date) do update set completed_tasks = performer_statistics.completed_tasks + EXCLUDED.completed_tasks
;
";
        
        #endregion
        
        #region public
        
        public override async Task HandleNewTask(
            ITaskObserverContext ctx)
        {
            using (var cmd = ctx.ConnectionWrapper.CreateCommand())
            {
                cmd.CommandText = SqlAddCreatedTaskManager;
                cmd.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                cmd.Parameters.Add(new NpgsqlParameter("user_id", ctx.TaskInfo.AuthorId));
                cmd.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date));
                cmd.Parameters.Add(new NpgsqlParameter("created_tasks", 1));
                cmd.Parameters.Add(new NpgsqlParameter("completed_tasks", (object)0));
            
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public override async Task HandleTransition(
            ITaskObserverContext ctx)
        {
            if (ctx.Transition.FinalState == DefaultTaskStates.Complete.Id
                && ctx.Credentials.Id == ctx.TaskInfo.AuthorId)
            {
                using (var cmd = ctx.ConnectionWrapper.CreateCommand())
                {
                    cmd.CommandText = SqlAddCompletedTaskManager;
                    cmd.Parameters.Add(new NpgsqlParameter("user_id", ctx.TaskInfo.AuthorId));
                    cmd.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date));
                    cmd.Parameters.Add(new NpgsqlParameter("completed_tasks", 1));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else if (ctx.Transition.FinalState == DefaultTaskStates.Delivered.Id
                && ctx.Credentials.Id == ctx.TaskInfo.PerformerId)
            {
                using (var cmd = ctx.ConnectionWrapper.CreateCommand())
                {
                    cmd.CommandText = SqlAddCompletedTaskPerformer;
                    cmd.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    cmd.Parameters.Add(new NpgsqlParameter("user_id", ctx.TaskInfo.PerformerId));
                    cmd.Parameters.Add(new NpgsqlParameter("date", DateTime.UtcNow.Date));
                    cmd.Parameters.Add(new NpgsqlParameter("completed_tasks", 1));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        
        #endregion
    }
}