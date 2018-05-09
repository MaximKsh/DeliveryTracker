using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Views;
using DeliveryTracker.Views.References;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Views
{
    public class ReferenceViewGroupTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IViewService viewService;
       
        public ReferenceViewGroupTest()
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
            
            var services = new ServiceCollection();
            this.serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.DefaultInvitationSettings)
                .AddSingleton(this.DefaultPasswordSettings)
                .AddSingleton(this.DefaultTokenSettings)
                .AddSingleton(accessor.Object)
                .AddSingleton<IReferenceService<Product>, ProductReferenceService>()
                .AddSingleton<IReferenceService<Client>, ClientReferenceService>()
                .AddSingleton<IReferenceService<PaymentType>, PaymentTypeReferenceService>()
                .AddSingleton<IReferenceService<Warehouse>, WarehouseReferenceService>()
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, ReferenceViewGroup>()
                .BuildServiceProvider();

            this.viewService = this.serviceProvider.GetService<IViewService>();

        }

        [Fact]
        public void ViewsList()
        {
            // Assert
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var viewsList = viewGroup.GetViewsList().Result;
            
            // Assert
            Assert.Equal(
                new [] {"ClientsView", "ProductsView", "PaymentTypesView", "WarehousesView"}.OrderBy(p => p), 
                viewsList.OrderBy(p => p));
        }
        
        [Fact]
        public async void Digest()
        {
            // Assert
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var result = await viewGroup.GetDigestAsync();
            
            // Assert
            Assert.True(result.Success);
        }
        
        [Fact]
        public async void Product()
        {
            // Arrange
            var productService = this.serviceProvider.GetService<IReferenceService<Product>>();
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            await productService.CreateAsync(product);
            await productService.CreateAsync(product);
            await productService.CreateAsync(product);
            await productService.CreateAsync(product);
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "ProductsView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(4, abstractResult.Result.Count);
        }
        
        [Fact]
        public async void PaymentType()
        {
            // Arrange
            var service = this.serviceProvider.GetService<IReferenceService<PaymentType>>();
            var elem = new PaymentType
            {
                Name = "Visa",
            };
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "PaymentTypesView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(4, abstractResult.Result.Count);
        }
        
        
        [Fact]
        public async void ClientType()
        {
            // Arrange
            var service = this.serviceProvider.GetService<IReferenceService<Client>>();
            var elem = new Client
            {
                Name = "Ivanov",
            };
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "ClientsView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(4, abstractResult.Result.Count);
        }
        
        [Fact]
        public async void Warehouses()
        {
            // Arrange
            var service = this.serviceProvider.GetService<IReferenceService<Warehouse>>();
            var elem = new Warehouse
            {
                Name = "Dom1",
            };
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            await service.CreateAsync(elem);
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "WarehousesView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(4, abstractResult.Result.Count);
        }
    }
}