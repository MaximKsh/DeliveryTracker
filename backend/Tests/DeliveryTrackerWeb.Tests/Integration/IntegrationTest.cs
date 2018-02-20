using System.Net;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class IntegrationTest : FunctionalTestBase
    {


        [Fact]
        public async void WebServiceAlive()
        {
            var client = this.Server.CreateClient();
            var result = await RequestGet(client, ServiceUrl(""));
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
        
        
        [Fact]
        public async void CreateThenGetInstance()
        {
            var (client, instance, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var getResult = await RequestGet<InstanceResponse>(
                client,
                InstanceUrl("get"));
            
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            Assert.Equal(instance.Id, getResult.Result.Instance.Id);
        }


        [Fact]
        public async void TestInvitations()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var inviteManagerResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Role = DefaultRoles.ManagerRole, }
                });
            
            var invitePerformerResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Surname = "Ivanov", Role = DefaultRoles.PerformerRole, }
                });

            var getInviteManagerResult = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={inviteManagerResult.Result.Invitation.InvitationCode}"));
            
            var getInvitePerformerResult = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={invitePerformerResult.Result.Invitation.InvitationCode}"));
            
            Assert.Equal(HttpStatusCode.OK, inviteManagerResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, invitePerformerResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getInviteManagerResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getInvitePerformerResult.StatusCode);
            Assert.Equal(inviteManagerResult.Result.Invitation.InvitationCode, getInviteManagerResult.Result.Invitation.InvitationCode);
            Assert.Equal(invitePerformerResult.Result.Invitation.InvitationCode, getInvitePerformerResult.Result.Invitation.InvitationCode);
            
            Assert.Equal("Ivanov", getInvitePerformerResult.Result.Invitation.PreliminaryUser.Surname);

            var deleteManagerInvitationResult = await RequestPost(
                client,
                InvitationUrl("delete"),
                new InvitationRequest { Code = inviteManagerResult.Result.Invitation.InvitationCode});
            var deletePerformerInvitationResult = await RequestPost(
                client,
                InvitationUrl("delete"),
                new InvitationRequest {Code = invitePerformerResult.Result.Invitation.InvitationCode});
            Assert.Equal(HttpStatusCode.OK, deleteManagerInvitationResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, deletePerformerInvitationResult.StatusCode);
            
            var getInviteManagerResultAfterDelete = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={inviteManagerResult.Result.Invitation.InvitationCode}"));
            
            var getInvitePerformerResultAfterDelete = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={invitePerformerResult.Result.Invitation.InvitationCode}"));
            
            Assert.Equal(HttpStatusCode.BadRequest, getInviteManagerResultAfterDelete.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, getInvitePerformerResultAfterDelete.StatusCode);
            Assert.All(getInviteManagerResultAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.InvitationNotFound, error.Code));
            Assert.All(getInvitePerformerResultAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.InvitationNotFound, error.Code));
        }

        [Fact]
        public async void InviteLoginAndGet()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, manager) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            
            var getResult = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            Assert.Equal(manager.Id, getResult.Result.User.Id);
        }

        [Fact]
        public async void RefreshSession()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var inviteResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        Role = DefaultRoles.ManagerRole,
                    }
                });

            var managerClient = this.Server.CreateClient();
            var firstLoginResult = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = inviteResult.Result.Invitation.InvitationCode,
                        Password = CorrectPassword,
                    }
                });
            SetToken(managerClient, firstLoginResult.Result.Token);


            var refreshResult1 = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("refresh"),
                new AccountRequest {RefreshToken = firstLoginResult.Result.RefreshToken});
            Assert.Equal(HttpStatusCode.OK, refreshResult1.StatusCode);
            
            SetToken(managerClient, refreshResult1.Result.Token);
            var refreshResult2 = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("refresh"),
                new AccountRequest {RefreshToken = firstLoginResult.Result.RefreshToken});
            Assert.Equal(HttpStatusCode.Forbidden, refreshResult2.StatusCode);
            
            SetToken(managerClient, firstLoginResult.Result.Token);
            var getResult1 = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.Forbidden, getResult1.StatusCode);
            
            SetToken(managerClient, refreshResult1.Result.Token);
            var getResult2 = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.OK, getResult2.StatusCode);
        }
        
        [Fact]
        public async void ManagerOperatePerformer()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            
            var (performerClient, performer) = await this.CreateUserViaInvitation(managerClient, DefaultRoles.PerformerRole);
            
            // Проверим от имени исполнителя что действительно создали
            var aboutPerformer1 = await RequestGet<AccountResponse>(
                performerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.OK, aboutPerformer1.StatusCode);
            
            // Получаем 
            var getPerformer = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            Assert.Equal(HttpStatusCode.OK, getPerformer.StatusCode);
            
            // Меняем имя
            var editPerformer = await RequestPost<AccountResponse>(
                managerClient,
                UserUrl("edit"),
                new AccountRequest
                {
                    User = new User { Id = performer.Id, Name = "NewUserName" }
                });
            Assert.Equal(HttpStatusCode.OK, editPerformer.StatusCode);
            
            // Получаем, проверяем новое имя
            var getPerformerWithNewName = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            Assert.Equal("NewUserName", getPerformerWithNewName.Result.User.Name);
            
            // Удаляем
            var deletePerformer = await RequestPost<AccountRequest>(
                managerClient,
                UserUrl("delete"),
                new UserRequest { Id = getPerformer.Result.User.Id});
            Assert.Equal(HttpStatusCode.OK, deletePerformer.StatusCode);
            
            // Не получаем
            var getPerformerAfterDelete = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            
            Assert.Equal(HttpStatusCode.BadRequest, getPerformerAfterDelete.StatusCode);
            Assert.All(getPerformerAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.UserNotFound, error.Code));
            
        }

        [Fact]
        public async void ChangePassword()
        {
            var (creatorClient, _, creator, _) = await this.CreateNewHttpClientAndInstance();

            var changePasswordResult = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("change_password"),
                new AccountRequest
                {
                    CodePassword = new CodePassword {Password = CorrectPassword},
                    NewCodePassword = new CodePassword {Password = CorrectPassword + CorrectPassword}
                });
            
            Assert.Equal(HttpStatusCode.OK, changePasswordResult.StatusCode);
            var loginResultOldPassword = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = creator.Code,
                        Password = CorrectPassword,
                    }
                });
            Assert.Equal(HttpStatusCode.Forbidden, loginResultOldPassword.StatusCode);
            
            var loginResult = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = creator.Code,
                        Password = CorrectPassword + CorrectPassword,
                    }
                });
            Assert.Equal(HttpStatusCode.OK, loginResult.StatusCode);
        }

        
    }
}