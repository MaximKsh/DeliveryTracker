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
    }
}