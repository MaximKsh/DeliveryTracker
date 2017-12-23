using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Models;
using DeliveryTracker.ViewModels;

namespace DeliveryTracker.Services
{
    public class PushMessageService
    {
        private readonly HttpClient client;

        private string key =
                "AAAArOgs9hw:APA91bGac5xib4PP18a_w5HV03uhTkdmMixv2IvXgPv6soDDzHWorT7AYfyCMTDzuZMgeMOQskTluBO-WT2yP9yb9mTDZ-mYgJLPmcpQmFwle0YAsb40UDy7FmX5eZ_-yw55qHRf7xdl"
            ;
        
        public PushMessageService()
        {
            this.client = new HttpClient();
            this.client.BaseAddress = new Uri("https://fcm.googleapis.com");

        }

        public async Task<ServiceResult> SendMessage(
            UserModel user,
            PushMessageViewModel pushMessage,
            Guid? taskId = null)
        {
            var requestBody = $@"
{{ 
    ""data"": {{
        ""msg"": ""{pushMessage.Content}"",
        ""taskId"": ""{taskId?.ToString() ?? string.Empty}""
    }}, 
    ""to"": ""{user.Device.FirebaseId}""
}}";

            try
            {
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var msg = new HttpRequestMessage();
                msg.Method = HttpMethod.Post;
                msg.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                msg.Headers.TryAddWithoutValidation("Authorization", $"key={this.key}");
                msg.RequestUri = new Uri("https://fcm.googleapis.com/fcm/send");
                var resp = await this.client.SendAsync(msg);
            }
            catch(HttpRequestException)
            {
            }
            
            return new ServiceResult();
        }
        
    }
}