using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IServiceProvider serviceProvider;

        public NotificationService(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }


        public ServiceResult SendNotification(
            INotification notification)
        {
            Task.Run(() =>
            {
                foreach (var notificator in this.serviceProvider.GetServices<INotificator>())
                {
                    foreach (var notificationComponent in notification.Components)
                    {
                        notificator.TryNotify(notificationComponent);
                    }
                }
            });
            return new ServiceResult();
        }
    }
}