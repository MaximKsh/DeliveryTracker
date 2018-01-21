using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.References
{
    public class ProductReferenceServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IReferenceService<Product> productService;

        public ProductReferenceServiceTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                var defaultInstance = TestHelper.CreateRandomInstance(conn);
                var me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.UserCredentials)
                    .Returns(new UserCredentials(me));
            }
            this.productService = new ProductReferenceService(this.Cp, accessor.Object);
        }

        [Fact]
        public async void Create()
        {
            // Arrange
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            
            // Act
            var createResult = await this.productService.CreateAsync(product);
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(product.Name, createResult.Result.Name);
            Assert.Equal(product.Description, createResult.Result.Description);
            Assert.Equal(product.Cost, createResult.Result.Cost);
            Assert.Equal(product.VendorCode, createResult.Result.VendorCode);
        }
        
        
        [Fact]
        public async void Get()
        {
            // Arrange
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            var createResult = await this.productService.CreateAsync(product);
            
            // Act
            var getResult = await this.productService.GetAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(getResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(product.Name, getResult.Result.Name);
            Assert.Equal(product.Description, getResult.Result.Description);
            Assert.Equal(product.Cost, getResult.Result.Cost);
            Assert.Equal(product.VendorCode, getResult.Result.VendorCode);
        }
        
        [Fact]
        public async void Edit()
        {
            // Arrange
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            var createResult = await this.productService.CreateAsync(product);
            var newProduct = new Product
            {
                Id = createResult.Result.Id,
                Name = "Sushi",
            };
            
            // Act
            var editResult = await this.productService.EditAsync(newProduct);
            
            // Assert
            Assert.True(editResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(newProduct.Name, editResult.Result.Name);
            Assert.Equal(product.Description, editResult.Result.Description);
            Assert.Equal(product.Cost, editResult.Result.Cost);
            Assert.Equal(product.VendorCode, editResult.Result.VendorCode);
        }
        
        
        [Fact]
        public async void Delete()
        {
            // Arrange
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            var createResult = await this.productService.CreateAsync(product);
            
            // Act
            var deleteResult = await this.productService.DeleteAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(deleteResult.Success, deleteResult.Errors.ErrorsToString());
        }
    }
}