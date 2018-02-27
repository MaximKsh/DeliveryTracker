using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Tasks
{
    public static class TaskExtensions
    {
        public static IServiceCollection AddDeliveryTrackerTaskStates(this IServiceCollection services)
        {
            services
                .AddScoped<TaskService>()
                .AddScoped<TaskStateCache>();
            
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
    }
}