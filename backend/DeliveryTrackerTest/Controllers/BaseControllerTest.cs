using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace DeliveryTrackerTest.Controllers
{
    public abstract class BaseControllerTest
    {
        
        protected readonly TestServer Server = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
/*
        protected const string ContentType ="application/json";
        
        protected static string PingUrl() =>
            "/";

        protected static string UserUrl(string command) =>
            $"/api/user/{command}";
        
        protected static string SessionUrl(string command) =>
            $"/api/session/{command}";
        
        protected static string InstanceUrl(string command) =>
            $"/api/instance/{command}";
        
        protected static string ManagerUrl(string command) =>
            $"/api/manager/{command}";
        
        protected static string PerformerUrl(string command) =>
            $"/api/performer/{command}";
        
        protected static async Task<UserViewModel> CreateInstance(
            HttpClient client,
            string creatorName,
            string creatorPassword,
            string instanceName,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var createInstanceRequest = new CreateInstanceViewModel
            {
                Creator = new UserViewModel()
                {
                    Surname = creatorName,
                    Name = creatorName,
                    Role = RoleAlias.Creator
                },
                Credentials = new CredentialsViewModel
                {
                    Password = creatorPassword
                },
                Instance = new InstanceViewModel
                {
                    InstanceName = instanceName
                },
            };
            var createInstanceContent = JsonConvert.SerializeObject(createInstanceRequest);
            var createInstanceResponse = await client.PostAsync(
                InstanceUrl("create"), 
                new StringContent(createInstanceContent, Encoding.UTF8, ContentType));
            Assert.Equal(expectStatusCode, createInstanceResponse.StatusCode);
            if (!createInstanceResponse.IsSuccessStatusCode)
            {
                return null;
            }
            
            var createInstanceResponseBody =  
                JsonConvert.DeserializeObject<UserViewModel>(await createInstanceResponse.Content.ReadAsStringAsync());
            
            Assert.Equal(RoleAlias.Creator, createInstanceResponseBody.Role);
            return createInstanceResponseBody;
        }

        protected static async Task<TokenViewModel> GetToken(
            HttpClient client,
            string username,
            string password,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            var loginRequest = new CredentialsViewModel
            {
                Username = username,
                Password = password,
            };
            var loginContent = JsonConvert.SerializeObject(loginRequest);
            var loginResponse = await client.PostAsync(
                SessionUrl("login"), 
                new StringContent(loginContent, Encoding.UTF8, ContentType));
            Assert.Equal(expectStatusCode, loginResponse.StatusCode);
            if (!loginResponse.IsSuccessStatusCode)
            {
                return null;
            }
            var tokenResponse =  
                JsonConvert.DeserializeObject<TokenViewModel>(await loginResponse.Content.ReadAsStringAsync());

            return tokenResponse ;
        }

        protected static async Task<string> Invite(
            HttpClient client,
            string role,
            string token,
            HttpStatusCode expectStatusCode = HttpStatusCode.OK)
        {
            string url;
            if (role == RoleAlias.Manager)
            {
                url = InstanceUrl("invite_manager");
            }
            else if (role == RoleAlias.Performer)
            {
                url = InstanceUrl("invite_performer");
            }
            else
            {
                throw new ArgumentException("role");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var loginContent = JsonConvert.SerializeObject(new UserViewModel());
            var response = await client.PostAsync(
                url,
                new StringContent(loginContent, Encoding.UTF8, ContentType));
            
            Assert.Equal(expectStatusCode, response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseBody =  
                JsonConvert.DeserializeObject<InvitationViewModel>(await response.Content.ReadAsStringAsync());

            return responseBody.InvitationCode;
        }

        protected static async Task<UserViewModel> CheckSession(HttpClient client,string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(SessionUrl("check"));
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            
            var createInstanceResponseBody =  
                JsonConvert.DeserializeObject<UserViewModel>(await response.Content.ReadAsStringAsync());
            return createInstanceResponseBody;
            
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
                    var invitationCode = await Invite(client, role, token);

                    await GetToken(client, invitationCode, password, HttpStatusCode.Created);
                    
                    return invitationCode;
                })
                .ToList();
            
            var strs = new List<string>();
            foreach (var username in users)
            {
                strs.Add(await username);
            }
            return strs;
        }
        */
    }
}