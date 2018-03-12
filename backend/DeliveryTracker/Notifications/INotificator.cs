using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public interface INotificator
    {
        ServiceResult<bool> TryNotify(
            INotificationComponent notification);

    }
}