using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Notifications
{
    public sealed class PushNotificator : NotificatorBase<PushNotificator.IPushNotificationComponent>
    {
        public interface IPushNotificationComponent : INotificationComponent
        {
            Device Device { get; set; }
            PushNotificationBody Body { get; set; }
        }

        private readonly HttpClient client;
        private readonly NotificationSettings settings;
        private readonly IDeliveryTrackerSerializer serializer;
        
        public PushNotificator(
            ISettingsStorage settingsStorage,
            IDeliveryTrackerSerializer serializer)
        {
            this.serializer = serializer;
            this.settings = settingsStorage.GetSettings<NotificationSettings>(SettingsName.Notifiation);

            this.client = new HttpClient();
        }
        

        public override ServiceResult Notify(
            IPushNotificationComponent notification)
        {
            if (string.IsNullOrWhiteSpace(this.settings.FirebaseServerKey)
                || string.IsNullOrWhiteSpace(notification.Device.FirebaseId))
            {
                return new ServiceResult();
            }

            var requestBody = new Dictionary<string, object>
            {
                ["data"] = notification.Body,
                ["to"] = notification.Device.FirebaseId,
            };
                
            var content = new StringContent(
                this.serializer.SerializeJson(requestBody), 
                Encoding.UTF8, 
                "application/json");
            var request = new HttpRequestMessage {Content = content};
            request.Headers.TryAddWithoutValidation("Authorization", $"key={this.settings.FirebaseServerKey}");
            request.RequestUri = new Uri("https://fcm.googleapis.com/fcm/send");
            request.Method = HttpMethod.Post;
            var resultTask = this.client.SendAsync(request);
            resultTask.Wait();
            return new ServiceResult();
        }
    }
}