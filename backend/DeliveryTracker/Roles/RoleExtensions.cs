using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Roles
{
    public static class RoleExtensions
    {
        public static IServiceCollection AddDeliveryTrackerRoles(this IServiceCollection services)
        {
            services
                .AddScoped<RoleCache>();
            
            return services;
        }
    }
}