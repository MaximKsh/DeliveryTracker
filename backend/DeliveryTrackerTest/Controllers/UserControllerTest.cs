namespace DeliveryTrackerTest.Controllers
{
    public class UserControllerTest: BaseControllerTest
    {
        /*
        [Fact]
        public async Task IsServerAvailable()
        {
            var client = this.Server.CreateClient();
            var response = await client.GetAsync(PingUrl());
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Modify()
        {
            var client = this.Server.CreateClient();
            var client1 = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            
            var obj1 = JsonConvert.SerializeObject(new UserViewModel
            {
                Surname = "Тест1",
                Name = "Тест2"
            });
            var response1 = await client.PostAsync(
                InstanceUrl("invite_performer"),
                new StringContent(obj1, Encoding.UTF8, ContentType));
            
            var responseBody1 =  
                JsonConvert.DeserializeObject<InvitationViewModel>(await response1.Content.ReadAsStringAsync());

            var invitationCode = responseBody1.InvitationCode;
            
            var performerToken = 
                await GetToken(client1, invitationCode, "123qQ!", HttpStatusCode.Created);
            Assert.Equal(invitationCode, performerToken.User.Username);
            Assert.Equal("Тест1", performerToken.User.Surname);
            Assert.Equal("Тест2", performerToken.User.Name);
            Assert.Equal(RoleAlias.Performer, performerToken.User.Role);
            Assert.Equal("Instance1", performerToken.User.Instance.InstanceName);
            Assert.NotNull(performerToken.Token);
            client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken.Token);
            
            var obj2 = JsonConvert.SerializeObject(new UserViewModel
            {
                Surname = "Тест3",
                Name = "Тест4"
            });
            await client1.PostAsync(
                UserUrl("modify"),
                new StringContent(obj2, Encoding.UTF8, ContentType));
            
            var response3 = await client.GetAsync(
                InstanceUrl($"get_user/{performerToken.User.Username}"));
            
            var responseBody3 =  
                JsonConvert.DeserializeObject<UserViewModel>(await response3.Content.ReadAsStringAsync());
            
            Assert.Equal(invitationCode, responseBody3.Username);
            Assert.Equal("Тест3", responseBody3.Surname);
            Assert.Equal("Тест4", responseBody3.Name);
            Assert.Equal(RoleAlias.Performer, responseBody3.Role);
            Assert.Equal("Instance1", responseBody3.Instance.InstanceName);

        }

        [Fact]
        public async Task ChangePassword()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            
            var obj1 = JsonConvert.SerializeObject(new ChangePasswordViewModel
            {
                CurrentCredentials = new CredentialsViewModel
                {
                    Password = "123qQ!"
                },
                NewCredentials = new CredentialsViewModel
                {
                    Password = "321qQ!"
                }
            });
            var response1 = await client.PostAsync(
                UserUrl("change_password"),
                new StringContent(obj1, Encoding.UTF8, ContentType));
            Assert.True(response1.IsSuccessStatusCode);
            
            await GetToken(client, creator.Username, "123qQ!", HttpStatusCode.Unauthorized);
            token = await GetToken(client, creator.Username, "321qQ!");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            
            var obj2 = JsonConvert.SerializeObject(new ChangePasswordViewModel
            {
                CurrentCredentials = new CredentialsViewModel
                {
                    Password = "123qQ!"
                },
                NewCredentials = new CredentialsViewModel
                {
                    Password = "321qQ!"
                }
            });
            var response2 = await client.PostAsync(
                UserUrl("change_password"),
                new StringContent(obj2, Encoding.UTF8, ContentType));
            Assert.False(response2.IsSuccessStatusCode);
        }
        */
    }
}