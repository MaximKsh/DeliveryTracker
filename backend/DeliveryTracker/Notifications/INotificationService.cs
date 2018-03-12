using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public interface INotificationService
    {
        ServiceResult SendNotification(
            INotification notification);
    }
}