using System.Net;
using DeliveryTracker.Geopositioning;
using DeliveryTracker.Identification;
using DeliveryTrackerWeb.ViewModels;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class GeopositioningTest : FunctionalTestBase
    {
        [Fact]
        public async void SendGeoposition()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (performerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            
            var result = await RequestPost<GeopositioningResponse>(
                performerClient,
                GeopositioningUrl("update"),
                new GeopositioningRequest()
                {
                    Geoposition = new Geoposition
                    {
                        Longitude = 37.9,
                        Latitude = 38.1,
                    }
                });
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
        
        [Fact]
        public async void SendNullGeoposition()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (performerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            
            var result = await RequestPost<GeopositioningResponse>(
                performerClient,
                GeopositioningUrl("update"),
                new GeopositioningRequest());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
        
        [Fact]
        public async void SendWrongRoleGeoposition()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var result = await RequestPost<GeopositioningResponse>(
                client,
                GeopositioningUrl("update"),
                new GeopositioningRequest());
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }
        
        [Fact]
        public async void SendUnauthorizedGeoposition()
        {
            await this.CreateNewHttpClientAndInstance();
            var newCLient = this.Server.CreateClient();
            var result = await RequestPost<GeopositioningResponse>(
                newCLient,
                GeopositioningUrl("update"),
                new GeopositioningRequest());
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }
    }
}