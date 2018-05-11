using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTrackerWeb.Tests.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Web;

namespace DeliveryTrackerWeb.Tests
{
    public abstract class FunctionalTestBase
    {
        protected readonly TestServer Server;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings();

        static FunctionalTestBase()
        {
            Settings.Converters.Add(new ErrorJsonConverter());
            Settings.Converters.Add(new DictionaryObjectJsonConverter());
            Settings.NullValueHandling = NullValueHandling.Ignore;
            Settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            Settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            Settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            Settings.Formatting = Formatting.None;
        }
        
        protected FunctionalTestBase()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            this.Server = new TestServer(new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<TestStartup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog());

            
        }

        protected const string ContentType ="application/json";

        protected const string CorrectInstanceName = "Тестовая Компания";
        
        protected const string CorrectPassword = "123Bb!";
        
        protected static string ServiceUrl(string command) =>
            $"/{command}";

        protected static string UserUrl(string command) =>
            $"/api/user/{command}";
        
        protected static string AccountUrl(string command) =>
            $"/api/account/{command}";
        
        protected static string GeopositioningUrl(string command) =>
            $"/api/geopositioning/{command}";
        
        protected static string InstanceUrl(string command) =>
            $"/api/instance/{command}";
        
        protected static string InvitationUrl(string command) =>
            $"/api/invitation/{command}";
        
        protected static string TaskUrl(string command) =>
            $"/api/tasks/{command}";
        
        protected static string ReferenceUrl(string command) =>
            $"/api/reference/{command}";
        
        protected static string ViewUrl(string command) =>
            $"/api/view/{command}";

        protected static void SetToken(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        protected static async Task<WebServiceResponse> RequestGet(
            HttpClient client,
            string url)
        {
            var response = await client.GetAsync(url);
            
            return new WebServiceResponse
            {
                HttpResponse = response
            };
        }
        
        protected static async Task<WebServiceResponse<T>> RequestGet<T>(
            HttpClient client,
            string url) where T : class
        {
            var response = await client.GetAsync(url);
            
            T result = null;
            if (response.Content.Headers.ContentLength != 0)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                result =  
                    JsonConvert.DeserializeObject<T>(responseString, Settings);
            }
            return new WebServiceResponse<T>
            {
                Result = result,
                HttpResponse = response
            };
        }
        
        protected static async Task<WebServiceResponse<T>> RequestPost<T>(
            HttpClient client,
            string url,
            params object[] payloadObjects) where T : class
        {
            object payload;
            switch (payloadObjects.Length)
            {
                case 0:
                    payload = string.Empty;
                    break;
                case 1:
                    payload = payloadObjects[0];
                    break;
                default:
                    payload = payloadObjects;
                    break;
            }
            var payloadString = JsonConvert.SerializeObject(payload, Settings);

            var payloadBytes = Encoding.UTF8.GetBytes(payloadString);
            var content = new ByteArrayContent(payloadBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentEncoding.Add("UTF-8");
            content.Headers.ContentLength = payloadBytes.Length;
            var response = await client.PostAsync(
                url, 
                content);
            T result = null;
            if (response.Content.Headers.ContentLength != 0)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                result =  
                    JsonConvert.DeserializeObject<T>(responseString, Settings);
            }
            return new WebServiceResponse<T>
            {
                Result = result,
                HttpResponse = response
            };
        }
        
        protected static async Task<WebServiceResponse> RequestPost(
            HttpClient client,
            string url,
            params object[] payloadObjects)
        {
            object payload;
            switch (payloadObjects.Length)
            {
                case 0:
                    payload = string.Empty;
                    break;
                case 1:
                    payload = payloadObjects[0];
                    break;
                default:
                    payload = payloadObjects;
                    break;
            }
            var payloadString = JsonConvert.SerializeObject(payload, Settings);
            
            var payloadBytes = Encoding.UTF8.GetBytes(payloadString);
            var content = new ByteArrayContent(payloadBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentEncoding.Add("UTF-8");
            content.Headers.ContentLength = payloadBytes.Length;
            var response = await client.PostAsync(
                url, 
                content);
            
            return new WebServiceResponse
            {
                HttpResponse = response
            };
        }
        
        protected async Task<(HttpClient, Instance, User, string)> CreateNewHttpClientAndInstance()
        {
            var client = this.Server.CreateClient();
            var createResult = await RequestPost<InstanceResponse>(
                client, 
                InstanceUrl("create"),
                new InstanceRequest
                {
                    Creator = new User
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        PhoneNumber = "+89123456789"
                    },
                    CodePassword = new CodePassword
                    {
                        Password = CorrectPassword
                    },
                    Instance = new Instance
                    {
                        Name = CorrectInstanceName,
                    },
                    CreatorDevice = new Device()
                    {
                        ApplicationType = "Unknown",
                        ApplicationVersion = "Unknown",
                        FirebaseId = null,
                        Language = "ru",
                        Type = "Unknown",
                        Version = "Unknown",
                    }
                        
                });
            SetToken(client, createResult.Result.Token);   
            return (client, createResult.Result.Instance, createResult.Result.Creator, createResult.Result.Token);
        }

        protected async Task<(HttpClient, User)> CreateUserViaInvitation(HttpClient client, Guid role)
        {
            var inviteResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        Role = role,
                    }
                });

            var newClient = this.Server.CreateClient();
            var firstLoginResult = await RequestPost<AccountResponse>(
                newClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = inviteResult.Result.Invitation.InvitationCode,
                        Password = CorrectPassword,
                    }
                });
            SetToken(newClient, firstLoginResult.Result.Token);

            return (newClient, firstLoginResult.Result.User);
        }
        
    }
}