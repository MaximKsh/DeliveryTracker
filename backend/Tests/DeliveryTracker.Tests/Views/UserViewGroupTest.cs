using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Views
{
    public class UserViewGroupTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IViewService viewService;

        private readonly Instance defaultInstance;
       
        public UserViewGroupTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                var me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(me));
            }
            
            var services = new ServiceCollection();
            this.serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.SettingsStorage)
                .AddSingleton(accessor.Object)
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, UserViewGroup>()
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInstanceService>().Object)
                .BuildServiceProvider();

            this.viewService = this.serviceProvider.GetService<IViewService>();

        }

        [Fact]
        public void ViewsList()
        {
            // Assert
            var viewGroup = this.viewService.GetViewGroup("UserViewGroup").Result;
            
            // Act
            var viewsList = viewGroup.GetViewsList().Result;
            
            // Assert
            Assert.Equal(
                new [] {"ManagersView", "PerformersView", "InvitationsView"}.OrderBy(p => p), 
                viewsList.OrderBy(p => p));
        }
        
        [Fact]
        public async void Digest()
        {
            // Assert
            var viewGroup = this.viewService.GetViewGroup("UserViewGroup").Result;
            
            // Act
            var result = await viewGroup.GetDigestAsync();
            
            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async void ManagersView()
        {
            // Arrange
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
            }
            var viewGroup = this.viewService.GetViewGroup("UserViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "ManagersView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(6, abstractResult.Result.Count);
        }
        
        [Fact]
        public async void PerformersView()
        {
            // Arrange
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
                TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
            }
            var viewGroup = this.viewService.GetViewGroup("UserViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "PerformersView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(5, abstractResult.Result.Count);
        }

        [Fact]
        public async void InvitationsView()
        {
            // Arrange
            var invitationService = this.serviceProvider.GetService<IInvitationService>();
            var userData = new User
            {
                Role = DefaultRoles.PerformerRole,
            };
            await invitationService.CreateAsync(userData);
            await invitationService.CreateAsync(userData);
            await invitationService.CreateAsync(userData);
            await invitationService.CreateAsync(userData);
            var viewGroup = this.viewService.GetViewGroup("UserViewGroup").Result;
            
            // Act
            var abstractResult = await viewGroup.ExecuteViewAsync(
                "InvitationsView", 
                new Dictionary<string, IReadOnlyList<string>>());

            // Assert
            Assert.True(abstractResult.Success);
            Assert.Equal(4, abstractResult.Result.Count);
        }
    }
}