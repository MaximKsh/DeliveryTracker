namespace DeliveryTrackerTest.Controllers
{
    public class SessionControllerTest: BaseControllerTest
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
        public async Task CreateInstanceAndLogin()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            Assert.Equal("Иванов И.И.", token.User.Surname);
            Assert.Equal(RoleAlias.Creator, token.User.Role);
            Assert.True(!string.IsNullOrWhiteSpace(token.Token));
            var user = await CheckSession(client, token.Token);
            Assert.Equal(creator.Username, user.Username);
            Assert.Equal(RoleAlias.Creator, user.Role);
        }
        
        [Fact]
        public async Task CreateInstanceAndInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ1", HttpStatusCode.Unauthorized);
            Assert.True(string.IsNullOrWhiteSpace(token?.Token));
            var user = await CheckSession(client, token?.Token);
            Assert.Null(user);
        }
        
        [Fact]
        public async Task LoginAfterInvitationManager()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            Assert.Equal("Иванов И.И.", token.User.Surname);
            Assert.Equal(RoleAlias.Creator, token.User.Role);
            var invitation = await Invite(client, RoleAlias.Manager, token.Token);
            var token2 = await GetToken(client, invitation, "123qQ!", HttpStatusCode.Created); 
            Assert.Equal(RoleAlias.Manager, token2.User.Role);
            Assert.True(!string.IsNullOrWhiteSpace(token2.Token));
        }
        
        [Fact]
        public async Task LoginAfterInvitationManagerInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            Assert.Equal("Иванов И.И.", token.User.Surname);
            Assert.Equal(RoleAlias.Creator, token.User.Role);
            var invitation = await Invite(client, RoleAlias.Manager, token.Token);
            var token2 = await GetToken(client, invitation, "123qQ!", HttpStatusCode.Created); 
            Assert.Equal(RoleAlias.Manager, token2.User.Role);
            token2 = await GetToken(client, invitation, "123qQ1", HttpStatusCode.Unauthorized); 
            Assert.True(string.IsNullOrWhiteSpace(token2?.Token));
        }
        
        [Fact]
        public async Task LoginAfterInvitationPerformer()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            Assert.Equal("Иванов И.И.", token.User.Surname);
            Assert.Equal(RoleAlias.Creator, token.User.Role);
            var invitation = await Invite(client, RoleAlias.Performer, token.Token);
            var token2 = await GetToken(client, invitation, "123qQ!", HttpStatusCode.Created); 
            Assert.Equal(RoleAlias.Performer, token2.User.Role);
            Assert.True(!string.IsNullOrWhiteSpace(token2.Token));
        }
        
        [Fact]
        public async Task LoginAfterInvitationPerformerInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            Assert.Equal("Иванов И.И.", token.User.Surname);
            Assert.Equal(RoleAlias.Creator, token.User.Role);
            var invitation = await Invite(client, RoleAlias.Performer, token.Token);
            var token2 = await GetToken(client, invitation, "123qQ!", HttpStatusCode.Created); 
            Assert.Equal(RoleAlias.Performer, token2.User.Role);
            token2 = await GetToken(client, invitation, "123qQ1", HttpStatusCode.Unauthorized); 
            Assert.True(string.IsNullOrWhiteSpace(token2?.Token));
        }
        
        
        [Fact]
        public async Task WrongToken()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            var session = await CheckSession(client, token.Token.ToLowerInvariant());
            Assert.Null(session);
        }
    */
    }
    
}