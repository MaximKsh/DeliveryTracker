using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Geopositioning
{
    public static class GeopositioningExtensions
    {
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerGeopositioning(
            this IServiceCollection services)
        {
            services
                .AddSingleton<IGeopositioningService, GeopositioningService>()
                ;

            return services;
        }
        #endregion
    }
}