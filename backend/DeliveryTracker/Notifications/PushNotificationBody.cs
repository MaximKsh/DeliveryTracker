namespace DeliveryTracker.Notifications
{
    public class PushNotificationBody
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public string DataType => this.Data.GetType().Name;
        public object Data { get; set; }
    }
}