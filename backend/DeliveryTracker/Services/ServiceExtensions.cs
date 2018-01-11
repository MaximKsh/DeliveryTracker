

using DeliveryTracker.Instances;
using DeliveryTracker.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDeliveryTrackerServices(this IServiceCollection services)
        {
            services
                .AddSingleton<PushMessageService>()
                    
                .AddScoped<AccountService>()
                .AddScoped<InstanceService>()
                .AddScoped<PerformerService>()
                .AddScoped<TaskService>()

                ;
            return services;
        }
    }
}