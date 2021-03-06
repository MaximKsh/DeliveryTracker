﻿using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Notifications;
using DeliveryTracker.References;
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
                .Setup(x => x.GetUserCredentials())
                .Returns(() => null);
            
            var services = new ServiceCollection();
            var serviceProvider = services
                .AddDeliveryTrackerDatabase()
                .AddSingleton(this.Cp)
                .AddSingleton(this.SettingsStorage)
                .AddSingleton(accessor.Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInstanceService>().Object)
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                .AddSingleton<IAccountService, AccountService>()
                .AddSingleton<IInstanceService, InstanceService>()
                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IReferenceService<PaymentType>, PaymentTypeReferenceService>()
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
                new Device(),
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
                .AddSingleton(this.SettingsStorage)
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
                new Device(),
                new CodePassword
                {
                    Password = TestHelper.CorrectPassword
                });
            
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(() => createResult.Result.Credentials);
            
            // Act
            var getResult = await instanceService.GetAsync();
            
            // Assert
            Assert.True(getResult.Success, createResult.Errors.ErrorsToString());

            Assert.Equal(getResult.Result.Instance.Id, createResult.Result.Instance.Id);
        }
        
    }
}