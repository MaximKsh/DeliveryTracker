using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker;
using DeliveryTracker.Roles;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public abstract class BaseControllerTest
    {
        protected readonly TestServer Server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());

        protected const string ContentType ="application/json";
        
        protected static string PingUrl() =>
            "/";

        protected static string SessionUrl(string command) =>
            $"/api/session/{command}";
        
        protected static string GroupUrl(string command) =>
            $"/api/group/{command}";
        
        protected static string ManagerUrl(string command) =>
            $"/api/manager/{command}";
        
        protected static string PerformerUrl(string command) =>
            $"/api/performer/{command}";
        
        protected static async Task<(string, string, string, string)> CreateGroup(
            HttpClient client,
            string creatorName,
            string creatorPassword,
            string groupName,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var createGroupRequest = new CreateGroupViewModel
            {
                CreatorDisplayableName = creatorName,
                CreatorPassword = creatorPassword,
                GroupName = groupName,
            };
            var createGroupContent = JsonConvert.SerializeObject(createGroupRequest);
            var createGroupResponse = await client.PostAsync(
                GroupUrl("create"), 
                new StringContent(createGroupContent, Encoding.UTF8, ContentType));
            Assert.Equal(expectStatusCode, createGroupResponse.StatusCode);
            if (!createGroupResponse.IsSuccessStatusCode)
            {
                return (null, null, null, null);
            }
            
            var createGroupResponseBody =  
                JsonConvert.DeserializeObject<UserInfoViewModel>(await createGroupResponse.Content.ReadAsStringAsync());
            
            Assert.Equal(RoleInfo.Creator, createGroupResponseBody.Role);
            return (
                createGroupResponseBody.UserName,
                createGroupResponseBody.DisplayableName,
                createGroupResponseBody.Role,
                createGroupResponseBody.Group);
        }

        protected static async Task<(string, string, string)> GetToken(
            HttpClient client,
            string username,
            string password,
            string role,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var loginRequest = new CredentialsViewModel
            {
                UserName = username,
                Password = password,
                Role = role,
            };
            var loginContent = JsonConvert.SerializeObject(loginRequest);
            var loginResponse = await client.PostAsync(
                SessionUrl("login"), 
                new StringContent(loginContent, Encoding.UTF8, ContentType));
            Assert.Equal(expectStatusCode, loginResponse.StatusCode);
            if (!loginResponse.IsSuccessStatusCode)
            {
                return (null, null, null);
            }
            var tokenResponse =  
                JsonConvert.DeserializeObject<TokenViewModel>(await loginResponse.Content.ReadAsStringAsync());

            return (tokenResponse.User.DisplayableName, tokenResponse.Token, tokenResponse.User.Role);
        }

        protected static async Task<(string, string, DateTime?, string)> Invite(
            HttpClient client,
            string role,
            string token,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            string url;
            if (role == RoleInfo.Manager)
            {
                url = GroupUrl("invite_manager");
            }
            else if (role == RoleInfo.Performer)
            {
                url = GroupUrl("invite_performer");
            }
            else
            {
                throw new ArgumentException("role");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(url);
            
            Assert.Equal(expectStatusCode, response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                return (null, null, null, null);
            }
            var responseBody =  
                JsonConvert.DeserializeObject<InvitationViewModel>(await response.Content.ReadAsStringAsync());

            return (
                responseBody.InvitationCode,
                responseBody.Role,
                responseBody.ExpirationDate,
                responseBody.GroupName);
        }

        protected static async Task<(string, string, string, string)> AcceptInvitation(
            HttpClient client,
            string invitationCode,
            string displayableName,
            string password,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var acceptInvitaitonRequest = new AcceptInvitationViewModel
            {
                DisplayableName = displayableName,
                InvitationCode = invitationCode,
                Password = password
            };
            
            var content = JsonConvert.SerializeObject(acceptInvitaitonRequest);
            var response = await client.PostAsync(
                GroupUrl("accept_invitation"), 
                new StringContent(content, Encoding.UTF8, ContentType));
            Assert.Equal(expectStatusCode, response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                return (null, null, null, null);
            }
            var createGroupResponseBody =  
                JsonConvert.DeserializeObject<UserInfoViewModel>(await response.Content.ReadAsStringAsync());
            return (
                createGroupResponseBody.UserName,
                createGroupResponseBody.DisplayableName,
                createGroupResponseBody.Role,
                createGroupResponseBody.Group);
        }
        
        protected static async Task<(string, string, string, string)> CheckSession(HttpClient client,string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(SessionUrl("check"));
            if (!response.IsSuccessStatusCode)
            {
                return (null, null, null, null);
            }
            
            var createGroupResponseBody =  
                JsonConvert.DeserializeObject<UserInfoViewModel>(await response.Content.ReadAsStringAsync());
            return (
                createGroupResponseBody.UserName,
                createGroupResponseBody.DisplayableName,
                createGroupResponseBody.Role,
                createGroupResponseBody.Group);
            
        }

        protected static async Task<List<string>> MassCreateUsers(
            HttpClient client,
            string token,
            string role,
            string password,
            int count)
        {
            var users = Enumerable
                .Range(0, count)
                .Select(p => p)
                .Select(async _ =>
                {
                    var (invitationCode, _, _, _) = 
                        await Invite(client, role, token);
            
                    var (userName1, _ ,_ ,_) = 
                        await AcceptInvitation(client, invitationCode, $"MassCreatedUser_{Guid.NewGuid():N}", password);

                    return userName1;
                })
                .ToList();
            
            var strs = new List<string>();
            foreach (var username in users)
            {
                strs.Add(await username);
            }
            return strs;
        }
        
    }
}