namespace DeliveryTracker.Notifications
{
    public class PushNotificationComponent : PushNotificator.IPushNotificationComponent
    {
        public string Type { get; } = nameof(PushNotificationComponent);
    }
}