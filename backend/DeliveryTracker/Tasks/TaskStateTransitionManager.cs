using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Tasks
{
    public class TaskStateTransitionManager : ITaskStateTransitionManager
    {
        #region sql

        private const string SqlCount = @"
select reltuples 
from pg_class 
where relname = 'task_state_transitions'
;
";
        
        private const string SqlCanTransit = @"
select null
from ""tasks""
where ""id"" = @task_id and exists(
    select null
    from ""task_state_transitions""
    where ""id"" = @transition_id and ""role"" = @role and ""initial_state"" = ""tasks"".""state_id""
)
;
";
        
        private const string SqlHasTransition = @"
select null
from ""task_state_transitions""
where role = @role and initial_state = @initial_state and final_state = @final_state
;
";
        
        private static readonly string SqlGetTransition = $@"
select {TaskHelper.GetTaskStateTransitionColumns()}
from ""task_state_transitions""
where role = @role and initial_state = @initial_state and final_state = @final_state
;
";
        
        private static readonly string SqlGetTransitions = $@"
select {TaskHelper.GetTaskStateTransitionColumns()}
from ""task_state_transitions""
where role = @role and initial_state = @initial_state
;
";
        
        #endregion
        
        #region fields

        private readonly IPostgresConnectionProvider cp;

        private readonly Lazy<int> taskStateTransitionsCountLazy;
        #endregion

        #region constructor

        public TaskStateTransitionManager(
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
            
            this.taskStateTransitionsCountLazy = new Lazy<int>(
                this.GetTaskStateTransitionsCount, LazyThreadSafetyMode.ExecutionAndPublication);
        }        

        #endregion

        #region implementation

        public async Task<ServiceResult<bool>> CanTransit(
            Guid taskId,
            Guid transitionId,
            Guid role,
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlCanTransit;
                    command.Parameters.Add(new NpgsqlParameter("role", role));
                    command.Parameters.Add(new NpgsqlParameter("transitionId", transitionId));
                    command.Parameters.Add(new NpgsqlParameter("taskId", taskId));

                    return new ServiceResult<bool>(await command.ExecuteNonQueryAsync() == 1);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<bool>> HasTransition(
            Guid role,
            Guid initialState,
            Guid finalState, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlHasTransition;
                    command.Parameters.Add(new NpgsqlParameter("role", role));
                    command.Parameters.Add(new NpgsqlParameter("initial_state", initialState));
                    command.Parameters.Add(new NpgsqlParameter("final_state", finalState));

                    return new ServiceResult<bool>(await command.ExecuteNonQueryAsync() == 1);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskStateTransition>> GetTransition(
            Guid role,
            Guid initialState,
            Guid finalState, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetTransition;
                    command.Parameters.Add(new NpgsqlParameter("role", role));
                    command.Parameters.Add(new NpgsqlParameter("initial_state", initialState));
                    command.Parameters.Add(new NpgsqlParameter("final_state", finalState));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<TaskStateTransition>(reader.GetTaskStateTransition());
                        }

                        return new ServiceResult<TaskStateTransition>(
                            ErrorFactory.IncorrectTaskStateTransition(role, initialState, finalState));
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IList<TaskStateTransition>>> GetTransitions(
            Guid role,
            Guid initialState, 
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetTransitions;
                    command.Parameters.Add(new NpgsqlParameter("role", role));
                    command.Parameters.Add(new NpgsqlParameter("initial_state", initialState));

                    var list = new List<TaskStateTransition>(this.taskStateTransitionsCountLazy.Value);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(reader.GetTaskStateTransition());
                        }
                    }
                    return new ServiceResult<IList<TaskStateTransition>>(list);
                }
            }
        }

        #endregion

        #region private

        private int GetTaskStateTransitionsCount()
        {
            using (var connWrapper = this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlCount;
                    var count = command.ExecuteScalar() as int?;

                    return count.HasValue && count.Value > 10
                        ? count.Value
                        : 10;
                }
            }
        }

        #endregion
        
    }
}