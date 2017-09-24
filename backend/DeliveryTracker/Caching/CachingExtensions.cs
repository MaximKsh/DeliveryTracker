using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Caching
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddDeliveryTrackerCaching(this IServiceCollection services)
        {
            services.AddScoped<RolesCache>();
            services.AddScoped<TaskStatesCache>();
            
            return services;
        }
    }
}