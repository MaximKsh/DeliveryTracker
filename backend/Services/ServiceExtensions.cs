

using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDeliveryTrackerServices(this IServiceCollection services)
        {
            services.AddScoped<AccountService>();
            services.AddScoped<GroupService>();
            return services;
        }
    }
}