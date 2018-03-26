using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public sealed class NotificationSettings : ISettings
    {
        public NotificationSettings(
            string name,
            string firebaseServerKey,
            string smsRuServerKey)
        {
            this.Name = name;
            this.FirebaseServerKey = firebaseServerKey;
            this.SmsRuServerKey = smsRuServerKey;
        }

        public string Name { get; }
        
        public string FirebaseServerKey { get; }
        
        public string SmsRuServerKey { get; }

    }
}