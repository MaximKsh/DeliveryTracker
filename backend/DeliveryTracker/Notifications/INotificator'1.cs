using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public interface INotificator <in T> : INotificator 
            where T : INotificationComponent
    {
        ServiceResult Notify(
            T notification);

    }
}