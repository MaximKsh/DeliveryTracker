using DeliveryTracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Views
{
    public static class ViewExtensions
    {
        public static IServiceCollection AddDeliveryTrackerViews(this IServiceCollection services)
        {
            services
                .AddScoped<IViewService<UserViewModel>, UserViewService>()
                .AddScoped<IViewService<TaskViewModel>, TaskViewService>()
                ;
            
            return services;
        }
    }
}