namespace DeliveryTracker.Notifications
{
    public sealed class SmsNotificationComponent : SmsNotificator.ISmsNotificationComponent
    {
        public string Type { get; } = nameof(SmsNotificationComponent);
    }
}