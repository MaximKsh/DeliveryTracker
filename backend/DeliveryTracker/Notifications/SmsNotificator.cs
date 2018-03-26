using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using DeliveryTracker.Common;

namespace DeliveryTracker.Notifications
{
    public class SmsNotificator : NotificatorBase<SmsNotificator.ISmsNotificationComponent>
    {
        public interface ISmsNotificationComponent : INotificationComponent
        {
            string Text { get; set; }
            string Phone { get; set; }
        }
        
        private readonly HttpClient client;
        private readonly NotificationSettings settings;
        private readonly IDeliveryTrackerSerializer serializer;
        
        public SmsNotificator(
            ISettingsStorage settingsStorage,
            IDeliveryTrackerSerializer serializer)
        {
            this.serializer = serializer;
            this.settings = settingsStorage.GetSettings<NotificationSettings>(SettingsName.Notifiation);

            this.client = new HttpClient();
        }
        

        public override ServiceResult Notify(
            ISmsNotificationComponent notification)
        {
            if (string.IsNullOrWhiteSpace(this.settings.SmsRuServerKey)
                || string.IsNullOrWhiteSpace(notification.Phone)
                || !Regex.IsMatch(notification.Phone, "7\\(?\\d{3}\\)\\d{3}-\\d{2}-\\d{2}")
                || string.IsNullOrWhiteSpace(notification.Text))
            {
                return new ServiceResult();
            }
            
            var builder = new UriBuilder("https://sms.ru/sms/send");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["api_id"] = this.settings.SmsRuServerKey;
            query["to"] = notification.Phone;
            query["msg"] = notification.Text;
            builder.Query = query.ToString();

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = builder.Uri;
            var resultTask = this.client.SendAsync(request);
            resultTask.Wait();
            return new ServiceResult();
        }
    }
}