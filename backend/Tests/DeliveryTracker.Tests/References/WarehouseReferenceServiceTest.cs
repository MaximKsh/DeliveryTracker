using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.References
{
    public class WarehouseReferenceServiceTest: DeliveryTrackerConnectionTestBase
    {
        private readonly IReferenceService<Warehouse> productService;

        public WarehouseReferenceServiceTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                var defaultInstance = TestHelper.CreateRandomInstance(conn);
                var me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(me));
            }
            this.productService = new WarehouseReferenceService(this.Cp, accessor.Object);
        }

        [Fact]
        public async void Create()
        {
            // Arrange
            var warehouse = new Warehouse
            {
                Name = "Dom1",
                RawAddress = "123",
            };
            
            // Act
            var createResult = await this.productService.CreateAsync(warehouse);
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(warehouse.Name, createResult.Result.Name);
        }
        
        
        [Fact]
        public async void Get()
        {
            // Arrange
            var warehouse = new Warehouse
            {
                Name = "Dom1",
                RawAddress = "123",
            };
            var createResult = await this.productService.CreateAsync(warehouse);
            
            // Act
            var getResult = await this.productService.GetAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(getResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(warehouse.Name, getResult.Result.Name);
        }
        
        [Fact]
        public async void Edit()
        {
            // Arrange
            var warehouse = new Warehouse()
            {
                Name = "Dom1",
            };
            var createResult = await this.productService.CreateAsync(warehouse);
            var newWarehouse = new Warehouse()
            {
                Id = createResult.Result.Id,
                Name = "dom2",
            };
            
            // Act
            var editResult = await this.productService.EditAsync(newWarehouse);
            
            // Assert
            Assert.True(editResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(newWarehouse.Name, editResult.Result.Name);
        }
        
        
        [Fact]
        public async void Delete()
        {
            // Arrange
            var product = new Warehouse
            {
                Name = "Dom3",
            };
            var createResult = await this.productService.CreateAsync(product);
            
            // Act
            var deleteResult = await this.productService.DeleteAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(deleteResult.Success, deleteResult.Errors.ErrorsToString());
        }
    }
}