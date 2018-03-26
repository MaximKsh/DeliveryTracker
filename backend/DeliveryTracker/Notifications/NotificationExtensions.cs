using DeliveryTracker.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Notifications
{
    public static class NotificationExtensions
    {
        
        #region IServiceCollection
        
        public static IServiceCollection AddDeliveryTrackerNotifications(this IServiceCollection services)
        {
            return services
                    .AddSingleton<INotificationService, NotificationService>()
                    .AddSingleton<INotificator, PushNotificator>()
                    .AddSingleton<INotificator, SmsNotificator>()
                ;

        }
        
        public static ISettingsStorage AddDeliveryTrackerNotificationSettings(
            this ISettingsStorage storage, 
            IConfiguration configuration)
        {
            
            var settings = NotificationHelper.ReadNotificationSettingsFromConf(configuration);

            return storage
                    .RegisterSettings(settings)
                ;
        }
        
        #endregion
        
    }
}