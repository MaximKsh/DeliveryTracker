using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Tasks
{
    public static class TaskExtensions
    {
        public static IServiceCollection AddDeliveryTrackerTasks(this IServiceCollection services)
        {
            services
                .AddSingleton<ITaskStateTransitionManager, TaskStateTransitionManager>()
                ;
            
            return services;
        }

        public static TaskState GetState(
            this TaskInfo tInfo)
        {
            return DefaultTaskStates.AllTaskStates.TryGetValue(tInfo.TaskStateId, out var state)
                ? state
                : new TaskState(tInfo.TaskStateId, tInfo.TaskStateName, tInfo.TaskStateCaption);
        }

        public static void SetState(
            this TaskInfo tInfo,
            TaskState state)
        {
            tInfo.TaskStateId = state.Id;
            tInfo.TaskStateName = state.Name;
            tInfo.TaskStateCaption = state.Caption;
        }
        
        #region IDataReader
        
        public static TaskStateTransition GetTaskStateTransition(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetTaskStateTransition(ref idx);
        }
        
        public static TaskStateTransition GetTaskStateTransition(this IDataReader reader, ref int idx)
        {
            return new TaskStateTransition
            {
                Id = reader.GetGuid(idx++),
                Role = reader.GetGuid(idx++),
                InitialState = reader.GetGuid(idx++),
                FinalState = reader.GetGuid(idx++),
                ButtonCaption = reader.GetString(idx++),
            };
        }
        
        #endregion
    }
}