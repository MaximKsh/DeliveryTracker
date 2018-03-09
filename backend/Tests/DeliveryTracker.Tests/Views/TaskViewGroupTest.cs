using System;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Views
{
    public class TaskViewGroupTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IViewService viewServiceManager;
        private readonly IViewService viewServicePerformer;

        private readonly Instance defaultInstance;
       
        public TaskViewGroupTest()
        {
            var accessorManager = new Mock<IUserCredentialsAccessor>();
            var accessorPerformer = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                var manager = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                var performer = TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
                accessorManager
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(manager));
                accessorPerformer
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(performer));
            }

            var managerServiceProvider = this.BuildServiceProvider(accessorManager);
            var performerServiceProvider = this.BuildServiceProvider(accessorPerformer);

            this.viewServiceManager = managerServiceProvider.GetService<IViewService>();
            this.viewServicePerformer = performerServiceProvider.GetService<IViewService>();
        }

        [Fact]
        public void ViewsListManager()
        {
            // Assert
            var viewGroup = this.viewServiceManager.GetViewGroup(nameof(TaskViewGroup)).Result;
            
            // Act
            var viewsList = viewGroup.GetViewsList().Result;
            
            // Assert
            Assert.Equal(
                new [] {nameof(TasksManagerView)}.OrderBy(p => p), 
                viewsList.OrderBy(p => p));
        }
        
        [Fact]
        public void ViewsListPerformer()
        {
            // Assert
            var viewGroup = this.viewServicePerformer.GetViewGroup(nameof(TaskViewGroup)).Result;
            
            // Act
            var viewsList = viewGroup.GetViewsList().Result;
            
            // Assert
            Assert.Equal(
                new [] {nameof(TasksPerformerView)}.OrderBy(p => p), 
                viewsList.OrderBy(p => p));
        }
        
        [Fact]
        public async void DigestManager()
        {
            // Assert
            var viewGroup = this.viewServiceManager.GetViewGroup(nameof(TaskViewGroup)).Result;
            
            // Act
            var result = await viewGroup.GetDigestAsync();
            
            // Assert
            Assert.True(result.Success);
        }
        
        [Fact]
        public async void DigestPerformer()
        {
            // Assert
            var viewGroup = this.viewServicePerformer.GetViewGroup(nameof(TaskViewGroup)).Result;
            
            // Act
            var result = await viewGroup.GetDigestAsync();
            
            // Assert
            Assert.True(result.Success);
        }


        private IServiceProvider BuildServiceProvider(Mock<IUserCredentialsAccessor> accessor)
        {
            var services = new ServiceCollection();
            return services
                .AddSingleton(this.Cp)
                .AddSingleton(this.SettingsStorage)
                .AddSingleton(accessor.Object)
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<IViewService, ViewService>()
                .AddSingleton<IViewGroup, TaskViewGroup>()
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInstanceService>().Object)
                .BuildServiceProvider();
        }
    }
}