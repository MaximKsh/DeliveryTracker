using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.TaskStates
{
    public static class TaskStateExtensions
    {
        public static IServiceCollection AddDeliveryTrackerTaskStates(this IServiceCollection services)
        {
            services
                .AddScoped<TaskStateCache>();
            
            return services;
        }
    }
}