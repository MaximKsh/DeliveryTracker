using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public sealed class PushNotificator : NotificatorBase<PushNotificator.IPushNotificationComponent>
    {
        public interface IPushNotificationComponent : INotificationComponent
        {
            
        }


        public override ServiceResult Notify(
            IPushNotificationComponent notification)
        {
            return new ServiceResult();
        }
    }
}