using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Roles;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class InstanceControllerTest: BaseControllerTest
    {
        
        [Fact]
        public async Task IsServerAvailable()
        {
            var client = this.Server.CreateClient();
            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task CreateInstance()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");

            Assert.NotNull(creator.Username);
            Assert.Equal("Иванов И.И.", creator.Surname);
            Assert.Equal(RoleInfo.Creator, creator.Role);
            Assert.Equal("Instance1", creator.Instance.InstanceName);
        }
        
        [Fact]
        public async Task CreateInstanceBadPassword()
        {
            var client = this.Server.CreateClient();
            
            await CreateInstance(client, "Иванов И.И.", "123", "Instance1", HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task CreateInstanceEmptyFields()
        {
            var client = this.Server.CreateClient();
            
            await CreateInstance(client, "", "", "", HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task CreateInstanceSameFields()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");

            Assert.NotNull(creator.Username);
            Assert.Equal("Иванов И.И.", creator.Surname);
            Assert.Equal(RoleInfo.Creator, creator.Role);
            Assert.Equal("Instance1", creator.Instance.InstanceName);
            
            var creator2 = 
               await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");

            Assert.NotNull(creator2.Username);
            Assert.Equal("Иванов И.И.", creator2.Surname);
            Assert.Equal(RoleInfo.Creator, creator2.Role);
            Assert.Equal("Instance1", creator2.Instance.InstanceName);
        }
        
        [Fact]
        public async Task InvitationManager()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var creatorToken = await GetToken(client, creator.Username, "123qQ!");
            
            var invitationCode = await Invite(client, RoleInfo.Manager, creatorToken.Token);
            Assert.NotNull(invitationCode);
            
            var managerToken = 
                await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.Created);
            Assert.Equal(invitationCode, managerToken.User.Username);
            Assert.Equal(RoleInfo.Manager, managerToken.User.Role);
            Assert.Equal("Instance1", managerToken.User.Instance.InstanceName);
            Assert.NotNull(managerToken.Token);
        }
        
        
        [Fact]
        public async Task InvitationPerformer()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var creatorToken = await GetToken(client, creator.Username, "123qQ!");
            
            var invitationCode = await Invite(client, RoleInfo.Performer, creatorToken.Token);
            Assert.NotNull(invitationCode);
            
            var performerToken = 
                await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.Created);
            Assert.Equal(invitationCode, performerToken.User.Username);
            Assert.Equal(RoleInfo.Performer, performerToken.User.Role);
            Assert.Equal("Instance1", performerToken.User.Instance.InstanceName);
            Assert.NotNull(performerToken.Token);
        }

        [Fact]
        public async Task CantInviteInvalidToken()
        {
            var client = this.Server.CreateClient();
            var invitationCode = 
                await Invite(client, RoleInfo.Performer, "", HttpStatusCode.Unauthorized);
            Assert.Null(invitationCode);
        }
        
        
        [Fact]
        public async Task AcceptInvitationTwice()
        {
            var client = this.Server.CreateClient();
            
            var creator = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var creatorToken = await GetToken(client, creator.Username, "123qQ!");
            
            var invitationCode = await Invite(client, RoleInfo.Manager, creatorToken.Token);
            Assert.NotNull(invitationCode);
            
            await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.Created);
            await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task CantInviteManagerByManager()
        {
            var client = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var creatorToken = await GetToken(client, creator.Username, "123qQ!");
            
            var invitationCode = 
                await Invite(client, RoleInfo.Manager, creatorToken.Token);
            
            var managerToken = 
                await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.Created);
            
            await Invite(client, RoleInfo.Manager, managerToken.Token, HttpStatusCode.Forbidden);
        }
        
        [Fact]
        public async Task CantInviteByPerformer()
        {
            var client = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var creatorToken = await GetToken(client, creator.Username, "123qQ!");
            
            var invitationCode = 
                await Invite(client, RoleInfo.Performer, creatorToken.Token);
            
            var performerToken = 
                await GetToken(client, invitationCode, "123qQ!", HttpStatusCode.Created);
            
            await Invite(client, RoleInfo.Performer, performerToken.Token, HttpStatusCode.Forbidden);
        }
        
    }
}