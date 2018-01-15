using System.Threading.Tasks;
using DeliveryTracker.Identification;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Views
{
    public static class ViewExtensions
    {
        public static IServiceCollection AddDeliveryTrackerViews(this IServiceCollection services)
        {
            services
                .AddScoped<IViewService<User>, UserViewService>()
                .AddScoped<IViewService<Task>, TaskViewService>()
                ;
            
            return services;
        }
    }
}