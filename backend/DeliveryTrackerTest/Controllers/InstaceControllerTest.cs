using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Roles;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class InstaceControllerTest: BaseControllerTest
    {
        [Fact]
        public async Task IsServerAvailable()
        {
            var client = this.Server.CreateClient();
            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task CreateGroup()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, role, groupName) = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");

            Assert.NotNull(userName);
            Assert.Equal("Иванов И.И.", displayableName);
            Assert.Equal(RoleInfo.Creator, role);
            Assert.Equal("Группа1", groupName);
        }
        
        [Fact]
        public async Task CreateGroupBadPassword()
        {
            var client = this.Server.CreateClient();
            
            await CreateInstance(client, "Иванов И.И.", "123", "Группа1", HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task CreateGroupEmptyFields()
        {
            var client = this.Server.CreateClient();
            
            await CreateInstance(client, "", "", "", HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task CreateGroupSameFields()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, role, groupName) = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");

            Assert.NotNull(userName);
            Assert.Equal("Иванов И.И.", displayableName);
            Assert.Equal(RoleInfo.Creator, role);
            Assert.Equal("Группа1", groupName);
            
            var (userName2, displayableName2, role2, groupName2) = 
               await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");

            Assert.NotNull(userName2);
            Assert.Equal("Иванов И.И.", displayableName2);
            Assert.Equal(RoleInfo.Creator, role2);
            Assert.Equal("Группа1", groupName2);
        }
        
        [Fact]
        public async Task InvitationManager()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            
            var (invitationCode, roleInvite, expirationDate, groupName1) = await Invite(client, RoleInfo.Manager, token);
            Assert.NotNull(invitationCode);
            Assert.Equal(RoleInfo.Manager, roleInvite);
            Assert.Equal("Группа1", groupName1);
            
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            Assert.Equal(invitationCode, userName1);
            Assert.Equal("Петров П.П.", displayableNameAccept);
            Assert.Equal(RoleInfo.Manager, roleAccept);
            Assert.Equal("Группа1", groupNameAccept);
        }
        
        
        [Fact]
        public async Task InvitationPerformer()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            
            var (invitationCode, roleInvite, expirationDate, groupName1) = await Invite(client, RoleInfo.Performer, token);
            Assert.NotNull(invitationCode);
            Assert.Equal(RoleInfo.Performer, roleInvite);
            Assert.Equal("Группа1", groupName1);
            
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            Assert.Equal(invitationCode, userName1);
            Assert.Equal("Петров П.П.", displayableNameAccept);
            Assert.Equal(RoleInfo.Performer, roleAccept);
            Assert.Equal("Группа1", groupNameAccept);
        }

        [Fact]
        public async Task InvitationDoesnotExist()
        {
            var client = this.Server.CreateClient();
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, "abcdefgsd", "Петров П.П.", "123qQ!", HttpStatusCode.NotFound);
            Assert.Null(userName1);
            Assert.Null(displayableNameAccept);
            Assert.Null(roleAccept);
            Assert.Null(groupNameAccept);
        }
        
        [Fact]
        public async Task CantInviteInvalidToken()
        {
            var client = this.Server.CreateClient();
            var (invitationCode, roleInvite, expirationDate, groupName1) = 
                await Invite(client, RoleInfo.Performer, "", HttpStatusCode.Unauthorized);
            Assert.Null(invitationCode);
            Assert.Null(roleInvite);
            Assert.Null(groupName1);
        }
        
        [Fact]
        public async Task AcceptInvitationTwice()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            
            var (invitationCode, roleInvite, expirationDate, groupName1) = await Invite(client, RoleInfo.Performer, token);
            Assert.NotNull(invitationCode);
            Assert.Equal(RoleInfo.Performer, roleInvite);
            Assert.Equal("Группа1", groupName1);
            
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            Assert.Equal(invitationCode, userName1);
            Assert.Equal("Петров П.П.", displayableNameAccept);
            Assert.Equal(RoleInfo.Performer, roleAccept);
            Assert.Equal("Группа1", groupNameAccept);
            
            await AcceptInvitation(client, invitationCode, "Сидоров П.П.", "123rR!", HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task CantInviteManagerByManager()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            
            var (invitationCode, roleInvite, expirationDate, groupName1) = 
                await Invite(client, RoleInfo.Manager, token);
            
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            
            var (displayableName2, token2, role2) = await GetToken(client, userName1, "123qQ!", roleAccept); 
            
            var (invitationCode2, roleInvite2, expirationDate2, groupName2) = 
                await Invite(client, RoleInfo.Manager, token2, HttpStatusCode.Forbidden);
            
            Assert.Null(invitationCode2);
            Assert.Null(roleInvite2);
            Assert.Null(groupName2);
        }
        
        [Fact]
        public async Task CantInviteByPerformer()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            
            var (invitationCode, roleInvite, expirationDate, groupName1) = 
                await Invite(client, RoleInfo.Performer, token);
            
            var (userName1, displayableNameAccept ,roleAccept ,groupNameAccept) = 
                await AcceptInvitation(client, invitationCode, "Петров П.П.", "123qQ!");
            
            var (displayableName2, token2, role2) = await GetToken(client, userName1, "123qQ!", roleAccept); 
            
            var (invitationCode2, roleInvite2, expirationDate2, groupName2) = 
                await Invite(client, RoleInfo.Performer, token2, HttpStatusCode.Forbidden);
            
            Assert.Null(invitationCode2);
            Assert.Null(roleInvite2);
            Assert.Null(groupName2);
        }
    }
}