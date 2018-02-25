using DeliveryTracker.Geopositioning;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Geopositioning
{
    public class GeopositioningServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IGeopositioningService geopositioningService;
        
        private readonly Instance defaultInstance;
        private readonly User me;

        public GeopositioningServiceTest() : base()
        {
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                this.me = TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
            }
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(this.me));
            
            this.geopositioningService = new GeopositioningService(this.Cp, accessor.Object);
        }

        [Fact]
        public async void UpdatePosition()
        {
            // Act
            var result = await this.geopositioningService.UpdateGeopositionAsync(new Geoposition
            {
                Longitude = 37.3,
                Latitude = 57.3,
            });
            
            // Arrange
            Assert.True(result.Success, result.Errors.ErrorsToString());
        }

    }
}