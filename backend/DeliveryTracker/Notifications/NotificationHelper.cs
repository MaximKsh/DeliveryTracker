using System;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Notifications
{
    public static class NotificationHelper
    {
        public static NotificationSettings ReadNotificationSettingsFromConf(
            IConfiguration configuration)
        {
            return new NotificationSettings(
                SettingsName.Notifiation,
                configuration.GetValue<string>("NotificationSettings:FirebaseKey", null),
                configuration.GetValue<string>("NotificationSettings:SmsRuKey", null));
        }
    }
}