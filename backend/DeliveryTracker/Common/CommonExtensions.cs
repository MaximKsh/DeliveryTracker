using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Common
{
    public static class CommonExtensions
    {
        public static IServiceCollection AddDeliveryTrackerCommon(
            this IServiceCollection services)
        {
            services
                .AddSingleton<ISettingsStorage, SettingsStorage>()
                .AddSingleton<IDeliveryTrackerSerializer, DeliveryTrackerSerializer>()
                ;

            return services;
        }
        
        #region Logger extensions

        public static void Trace<T>(this ILogger<T> logger, string username, string message)
        {
            logger.LogTrace($"[{username}]: {message}");
        }
        
        #endregion
        
    }
}