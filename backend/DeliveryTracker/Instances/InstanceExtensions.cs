using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Instances
{
    public static class InstanceExtensions
    {
        public static IServiceCollection AddInstances(this IServiceCollection services)
        {
            services
                .AddScoped<AccountService>()
                .AddScoped<InstanceService>()
                .AddScoped<RoleCache>();
            
            return services;
        }
    }
}