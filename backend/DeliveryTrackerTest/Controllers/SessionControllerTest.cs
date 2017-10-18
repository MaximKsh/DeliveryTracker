using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Roles;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class SessionControllerTest: BaseControllerTest
    {
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
            
            var (userName, _, creatorRole, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (displayableName, token, role) = await GetToken(client, userName, "123qQ!", creatorRole);
            Assert.Equal("Иванов И.И.", displayableName);
            Assert.Equal(RoleInfo.Creator, role);
            Assert.True(!string.IsNullOrWhiteSpace(token));
            var (checkedUsername, _, _, _) = await CheckSession(client, token);
            Assert.Equal(userName, checkedUsername);
        }
        
        [Fact]
        public async Task CreateInstanceAndInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, role, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (_, token, _) = await GetToken(client, userName, "123qQ1", role, HttpStatusCode.Unauthorized);
            Assert.True(string.IsNullOrWhiteSpace(token));
            var (checkedUsername, _, _, _) = await CheckSession(client, token);
            Assert.Null(checkedUsername);
        }
        
        [Fact]
        public async Task LoginAfterInvitationManager()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, creatorRole, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (displayableName, token, role) = await GetToken(client, userName, "123qQ!", creatorRole);
            Assert.Equal("Иванов И.И.", displayableName);
            Assert.Equal(RoleInfo.Creator, role);
            var (invitationCode,_ ,_ ,_) = await Invite(client, RoleInfo.Manager, token);
            var (userName1,_ ,role1 ,_) = await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            var (displayableName2, token2, role2) = await GetToken(client, userName1, "123qQ!", role1); 
            Assert.Equal("Петров П.П.", displayableName2);
            Assert.Equal(RoleInfo.Manager, role2);
            Assert.True(!string.IsNullOrWhiteSpace(token2));
        }
        
        [Fact]
        public async Task LoginAfterInvitationManagerInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, creatorRole, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (displayableName, token, role) = await GetToken(client, userName, "123qQ!", creatorRole);
            Assert.Equal("Иванов И.И.", displayableName);
            Assert.Equal(RoleInfo.Creator, role);
            var (invitationCode,_ ,_ ,_) = await Invite(client, RoleInfo.Manager, token);
            await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            var (_, token2, _) = await GetToken(client, userName, "123qQ1", role, HttpStatusCode.Unauthorized); 
            Assert.True(string.IsNullOrWhiteSpace(token2));
        }
        
        [Fact]
        public async Task LoginAfterInvitationPerformer()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, role, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", role);
            var (invitationCode,_ ,_ ,_) = await Invite(client, RoleInfo.Performer, token);
            await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            var (_, token2, _) = await GetToken(client, userName, "123qQ!", role); 
            Assert.True(!string.IsNullOrWhiteSpace(token2));
        }
        
        [Fact]
        public async Task LoginAfterInvitationPerformerInvalidPassword()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, role, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", role);
            var (invitationCode,_ ,_ ,_) = await Invite(client, RoleInfo.Performer, token);
            await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            var (_, token2, _) = await GetToken(client, userName, "123qQ1", role, HttpStatusCode.Unauthorized); 
            Assert.True(string.IsNullOrWhiteSpace(token2));
        }
        
        
        [Fact]
        public async Task WrongToken()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, role, _) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", role);
            var (checkedUsername, _, _, _) = await CheckSession(client, token.ToLowerInvariant());
            Assert.Null(checkedUsername);
        }
    }
}