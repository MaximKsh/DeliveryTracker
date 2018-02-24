using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Instances
{
    public class AccountServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IAccountService accountService;
        
        private readonly Instance defaultInstance;
        private readonly User me;

        public AccountServiceTest() : base()
        {
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                this.me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
            }
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(this.me));
            
            var services = new ServiceCollection();
            this.serviceProvider = services
                .AddSingleton(this.Cp)
                .AddSingleton(this.SettingsStorage)
                .AddSingleton(accessor.Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IInvitationService>().Object)
                .AddSingleton(TestHelper.CreateLoggerMock<IAccountService>().Object)
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<IInvitationService, InvitationService>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                .AddSingleton<IAccountService, AccountService>()
                .BuildServiceProvider();

            this.accountService = this.serviceProvider.GetService<IAccountService>();
        }

        public static IEnumerable<object[]> DataForRegister()
        {
            yield return new object[]
            {
                DefaultRoles.ManagerRole
            };
            yield return new object[]
            {
                DefaultRoles.PerformerRole
            };
        }
        
        [Theory]
        [MemberData(nameof(DataForRegister))]
        public async void Register(Guid role)
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            
            // Act
            var result = await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = role;
                });
            
            // Arrange
            Assert.True(result.Success, result.Errors.ErrorsToString());
        }

        [Fact]
        public async void CantRegisterCreator()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            
            // Act
            var result = await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = DefaultRoles.CreatorRole;
                });
            
            // Arrange
            Assert.False(result.Success, result.Errors.ErrorsToString());
        }
        
        [Theory]
        [InlineData("", "Petrov", "Ivan", true)]
        [InlineData(null, "Petrov", "Ivan", true)]
        [InlineData(TestHelper.CorrectPassword, "Petrov", "Ivan", false)]
        public async void CantRegisterWrongData(string password, string surname, string name, bool correctInstance)
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = password,
            };
            
            // Act
            var result = await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = surname;
                    u.Name = name;
                    u.InstanceId = correctInstance ? this.defaultInstance.Id : Guid.NewGuid();
                    u.Role = DefaultRoles.ManagerRole;
                });
            
            // Arrange
            Assert.False(result.Success, result.Errors.ErrorsToString());
        }

        [Fact]
        public async void Login()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = DefaultRoles.ManagerRole;
                });
            
            // Act
            var result = await this.accountService.LoginAsync(codePassword);
            
            // Assert
            Assert.True(result.Success, result.Errors.ErrorsToString());
        }

        [Fact]
        public async void LoginWrongPassword()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = DefaultRoles.ManagerRole;
                });
            var newCodePassword = new CodePassword
            {
                Code = codePassword.Code,
                Password = TestHelper.CorrectPassword + TestHelper.CorrectPassword,
            };
            
            // Act
            var result = await this.accountService.LoginAsync(newCodePassword);
            
            // Assert
            Assert.False(result.Success, result.Errors.ErrorsToString());
        }

        [Fact]
        public async void LoginWithRegistrationRegister()
        {
            // Arrange
            var invitationService = this.serviceProvider.GetService<IInvitationService>();
            var userData = new User
            {
                Role = DefaultRoles.PerformerRole,
            };
            var invitationResult = await invitationService.CreateAsync(userData);
            var codePassword = new CodePassword
            {
                Code = invitationResult.Result.InvitationCode,
                Password = TestHelper.CorrectPassword,
            };

            // Act
            var loginResult = await this.accountService.LoginWithRegistrationAsync(codePassword);

            // Assert
            Assert.True(loginResult.Success, loginResult.Errors.ErrorsToString());
            Assert.True(loginResult.Result.Registered);
        }
        
        [Fact]
        public async void LoginWithRegistrationJustLogin()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            var result = await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = DefaultRoles.PerformerRole;
                });
            
            var codePasswordAfterRegistration = new CodePassword
            {
                Code = result.Result.User.Code,
                Password = TestHelper.CorrectPassword,
            };

            // Act
            var loginResult = await this.accountService.LoginWithRegistrationAsync(codePasswordAfterRegistration);

            // Assert
            Assert.True(loginResult.Success, loginResult.Errors.ErrorsToString());
        }
        
        [Fact]
        public async void LoginWithRegistrationInvalidPassword()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            var result = await this.accountService.RegisterAsync(
                codePassword,
                u =>
                {
                    u.Surname = "Petrov";
                    u.Name = "Ivan";
                    u.InstanceId = this.defaultInstance.Id;
                    u.Role = DefaultRoles.PerformerRole;
                });
            
            var codePasswordAfterRegistration = new CodePassword
            {
                Code = result.Result.User.Code,
                Password = TestHelper.CorrectPassword + "ccc",
            };

            // Act
            var loginResult = await this.accountService.LoginWithRegistrationAsync(codePasswordAfterRegistration);

            // Assert
            Assert.False(loginResult.Success, loginResult.Errors.ErrorsToString());
        }
        
        [Fact]
        public async void LoginWithRegistrationInvitationAndUserDontExist()
        {
            // Arrange
            var codePassword = new CodePassword
            {
                Code = Guid.NewGuid().ToString().Substring(0, 10),
                Password = TestHelper.CorrectPassword,
            };
            // Act
            var loginResult = await this.accountService.LoginWithRegistrationAsync(codePassword);

            // Assert
            Assert.False(loginResult.Success, loginResult.Errors.ErrorsToString());
        }

        [Fact]
        public async void GetMe()
        {
            // Act
            var getResult = await this.accountService.GetAsync();
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
        }
        
        [Fact]
        public async void EditMe()
        {
            // Arrange
            var newName = new User
            {
                Id = Guid.NewGuid(),
                Name = "Veniamin",
            };
            
            // Act
            var editResult = await this.accountService.EditAsync(newName);

            // Assert
            Assert.True(editResult.Success, editResult.Errors.ErrorsToString());
            Assert.Equal(this.me.Id, editResult.Result.User.Id);
            Assert.Equal("Veniamin", editResult.Result.User.Name);
        }
   
    }
}