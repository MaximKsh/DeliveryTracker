using DeliveryTracker.Identification;

namespace DeliveryTracker.Notifications
{
    public class PushNotificationComponent : PushNotificator.IPushNotificationComponent
    {
        public string Type { get; } = nameof(PushNotificationComponent);
        public Device Device { get; set; }
        public PushNotificationBody Body { get; set; }
    }
}