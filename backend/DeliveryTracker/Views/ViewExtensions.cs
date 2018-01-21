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
                .AddSingleton<IViewService, ViewService>()
                ;
            
            return services;
        }
    }
}