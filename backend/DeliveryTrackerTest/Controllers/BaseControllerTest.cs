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
        
        protected static async Task<(string, string, string, string)> CreateGroup(
            HttpClient client,
            string creatorName,
            string creatorPassword,
            string groupName,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var createGroupRequest = new CreateGroupRequestViewModel
            {
                CreatorDisplayableName = creatorName,
                CreatorPassword = creatorPassword,
                GroupName = groupName,
            };
            var createGroupContent = JsonConvert.SerializeObject(createGroupRequest);
            var createGroupResponse = await client.PostAsync(
                "/groups/create", 
                new StringContent(createGroupContent, Encoding.UTF8, "application/json"));
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
            var loginRequest = new LoginRequestViewModel
            {
                UserName = username,
                Password = password,
                Role = role,
            };
            var loginContent = JsonConvert.SerializeObject(loginRequest);
            var loginResponse = await client.PostAsync(
                "/session/login", 
                new StringContent(loginContent, Encoding.UTF8, "application/json"));
            Assert.Equal(expectStatusCode, loginResponse.StatusCode);
            if (!loginResponse.IsSuccessStatusCode)
            {
                return (null, null, null);
            }
            var loginResponseBody =  
                JsonConvert.DeserializeObject<LoginResponseViewModel>(await loginResponse.Content.ReadAsStringAsync());

            return (loginResponseBody.DisplayableName, loginResponseBody.Token, loginResponseBody.Role);
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
                url = "/groups/invite_manager";
            }
            else if (role == RoleInfo.Performer)
            {
                url = "/groups/invite_performer";
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
                JsonConvert.DeserializeObject<InvitationResponseViewModel>(await response.Content.ReadAsStringAsync());

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
            var acceptInvitaitonRequest = new AcceptInvitationRequestViewModel
            {
                DisplayableName = displayableName,
                InvitationCode = invitationCode,
                Password = password
            };
            
            var content = JsonConvert.SerializeObject(acceptInvitaitonRequest);
            var response = await client.PostAsync(
                "/groups/accept_invitation", 
                new StringContent(content, Encoding.UTF8, "application/json"));
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
            var response = await client.GetAsync("/session/check");
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
                        await AcceptInvitation(client, invitationCode, $"MassCreatedUser_{count}", password);

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