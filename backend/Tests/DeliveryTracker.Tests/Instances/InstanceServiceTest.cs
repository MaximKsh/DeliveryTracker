using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Instances
{
    public class InstanceServiceTest : DeliveryTrackerConnectionTestBase
    {

        [Fact]
        public async void Create()
        {
            // Arrange
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.UserCredentials)
                .Returns(() => null);
            
            var services = new ServiceCollection();
            var serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.DefaultInvitationSettings)
                .AddSingleton(this.DefaultPasswordSettings)
                .AddSingleton(this.DefaultTokenSettings)
                .AddSingleton(accessor.Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInstanceService>().Object)
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IInstanceService, InstanceService>()
                .BuildServiceProvider();

            var instanceService = serviceProvider.GetService<IInstanceService>();
            
            // Act
            var result = await instanceService.CreateAsync(
                "Компания1",
                new User
                {
                    Surname = "Petrov",
                    Name = "Ivanov",
                    PhoneNumber = "111"
                },
                new CodePassword
                {
                    Password = TestHelper.CorrectPassword
                });
            
            // Assert
            Assert.True(result.Success, result.Errors.ErrorsToString());

        }
        
        
        [Fact]
        public async void Get()
        {
            // Arrange
            var accessor = new Mock<IUserCredentialsAccessor>();
            var services = new ServiceCollection();
            var serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.DefaultInvitationSettings)
                .AddSingleton(this.DefaultPasswordSettings)
                .AddSingleton(this.DefaultTokenSettings)
                .AddSingleton(accessor.Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInstanceService>().Object)
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IInstanceService, InstanceService>()
                .BuildServiceProvider();

            var instanceService = serviceProvider.GetService<IInstanceService>();
            
            var createResult = await instanceService.CreateAsync(
                "Компания1",
                new User
                {
                    Surname = "Petrov",
                    Name = "Ivanov",
                    PhoneNumber = "111"
                },
                new CodePassword
                {
                    Password = TestHelper.CorrectPassword
                });
            
            accessor
                .Setup(x => x.UserCredentials)
                .Returns(() => createResult.Result.Item3);
            
            // Act
            var getResult = await instanceService.GetAsync();
            
            // Assert
            Assert.True(getResult.Success, createResult.Errors.ErrorsToString());

            Assert.Equal(getResult.Result.Id, createResult.Result.Item1.Id);
        }
        
    }
}