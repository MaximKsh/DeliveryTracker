using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public sealed class NotificationSettings : ISettings
    {
        public NotificationSettings(
            string name,
            string firebaseServerKey)
        {
            this.Name = name;
            this.FirebaseServerKey = firebaseServerKey;
        }

        public string Name { get; }
        
        public string FirebaseServerKey { get; }

    }
}