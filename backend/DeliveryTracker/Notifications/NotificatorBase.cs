using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public abstract class NotificatorBase<T> : INotificator<T> where T : INotificationComponent
    {
        public virtual ServiceResult<bool> TryNotify(
            INotificationComponent notification)
        {
            if (notification is T correctNotification)
            {
                var result = this.Notify(correctNotification);
                return result.Success
                    ? new ServiceResult<bool>(true)
                    : new ServiceResult<bool>(result.Errors);
            }

            return new ServiceResult<bool>(true);
        }

        public virtual ServiceResult Notify(
            T notification)
        {
            return new ServiceResult();
        }
    }
}