using System.Linq;
using System.Net;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTrackerWeb.ViewModels;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class ViewTest : FunctionalTestBase
    {

        [Fact]
        public async void TestGroupsList()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();

            var groupsListResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("groups"));
            
            Assert.Equal(HttpStatusCode.OK, groupsListResult.StatusCode);
            Assert.Equal(
                new [] {"UserViewGroup", "ReferenceViewGroup"}.OrderBy(p => p), 
                groupsListResult.Result.Groups.OrderBy(p => p));

        }

        [Fact]
        public async void TestUserGroup()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Role = DefaultRoles.ManagerRole, }
                });
            
            await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Surname = "Ivanov", Role = DefaultRoles.PerformerRole, }
                });

            var managersResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("UserViewGroup/ManagersView"));
            Assert.Equal(HttpStatusCode.OK, managersResult.StatusCode);
            Assert.Equal(2, managersResult.Result.ViewResult.Count());
            
            var performersResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("UserViewGroup/PerformersView"));
            Assert.Equal(HttpStatusCode.OK, performersResult.StatusCode);
            Assert.Equal(1, performersResult.Result.ViewResult.Count());
            
            var invitationResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("UserViewGroup/InvitationsView"));
            Assert.Equal(HttpStatusCode.OK, invitationResult.StatusCode);
            Assert.Equal(2, invitationResult.Result.ViewResult.Count());
            
            
            var digestResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("UserViewGroup/digest"));
            Assert.Equal(HttpStatusCode.OK, digestResult.StatusCode);
            Assert.Equal(3, digestResult.Result.Digest.Count);
            Assert.Equal(2, digestResult.Result.Digest["ManagersView"]);
            Assert.Equal(1, digestResult.Result.Digest["PerformersView"]);
            Assert.Equal(2, digestResult.Result.Digest["InvitationsView"]);
        }

        [Fact]
        public async void TestReferenceGroup()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl("Client/create"),
                new ReferenceRequest
                {
                    Entity = new Client { Surname = "Petrov", }.GetDictionary()
                });
            
            await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl("Product/create"),
                new ReferenceRequest
                {
                    Entity = new Product { Name = "pizza", }.GetDictionary()
                });
            
            await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl("PaymentType/create"),
                new ReferenceRequest
                {
                    Entity = new PaymentType { Name = "Visa", }.GetDictionary()
                });

            var clientsResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("ReferenceViewGroup/ClientsView"));
            Assert.Equal(HttpStatusCode.OK, clientsResult.StatusCode);
            Assert.Equal(1, clientsResult.Result.ViewResult.Count());
            
            var productsResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("ReferenceViewGroup/ProductsView"));
            Assert.Equal(HttpStatusCode.OK, productsResult.StatusCode);
            Assert.Equal(1, productsResult.Result.ViewResult.Count());
            
            var paymentTypeResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("ReferenceViewGroup/PaymentTypesView"));
            Assert.Equal(HttpStatusCode.OK, paymentTypeResult.StatusCode);
            Assert.Equal(1, paymentTypeResult.Result.ViewResult.Count());
            
            
            var digestResult = await RequestGet<ViewResponse>(
                client,
                ViewUrl("ReferenceViewGroup/digest"));
            Assert.Equal(HttpStatusCode.OK, digestResult.StatusCode);
            Assert.Equal(3, digestResult.Result.Digest.Count);
            Assert.Equal(1, digestResult.Result.Digest["ClientsView"]);
            Assert.Equal(1, digestResult.Result.Digest["ProductsView"]);
            Assert.Equal(1, digestResult.Result.Digest["PaymentTypesView"]);
        }
    }
}