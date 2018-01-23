using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.References;
using DeliveryTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Views
{
    public class ViewsTest: DeliveryTrackerConnectionTestBase
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IViewService viewService;
       
        public ViewsTest()
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
            
            var services = new ServiceCollection();
            this.serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.DefaultInvitationSettings)
                .AddSingleton(this.DefaultPasswordSettings)
                .AddSingleton(this.DefaultTokenSettings)
                .AddSingleton(accessor.Object)
                .AddSingleton<IReferenceService<Product>, ProductReferenceService>()
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, ReferenceViewGroup>()
                .BuildServiceProvider();

            this.viewService = this.serviceProvider.GetService<IViewService>();

        }

        [Fact]
        public async void Create()
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
            
            // Act
            var viewGroup = this.viewService.GetViewGroup("ReferenceViewGroup").Result;
            var viewsList = viewGroup.GetViewsList();
            var digest = await viewGroup.GetDigestAsync();
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "ProductsView", 
                new Dictionary<string, string[]>().ToImmutableDictionary());
            var typifiedResult = await viewGroup.ExecuteViewAsync<Product>(
                "ProductsView",
                new Dictionary<string, string[]>().ToImmutableDictionary());

            // Assert
        }
    }
}