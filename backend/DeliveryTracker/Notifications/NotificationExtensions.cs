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
        
        #endregion
        
    }
}