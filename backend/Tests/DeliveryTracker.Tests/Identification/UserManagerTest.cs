using System;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Xunit;

namespace DeliveryTracker.Tests.Identification
{
    public class UserManagerTest : DeliveryTrackerTestBase
    {
        private readonly IUserManager userManager;
        
        /// <summary>
        /// Инстанс, для которого тестируются пользователи.
        /// </summary>
        private readonly Instance instance;
        
        public UserManagerTest() : base()
        {
            var provider = new PostgresConnectionProvider(this.Configuration);
            this.userManager = new UserManager(provider);
            using (var conn = provider.Create())
            {
                conn.Connect();
                this.instance = TestHelper.CreateRandomInstance(conn);
            }
        }

        [Fact]
        public async void AddUser()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            var newUserResult = await this.userManager.CreateAsync(user);
            Assert.True(newUserResult.Success);
            var newUser = newUserResult.Result;
            Assert.Equal(user.Id, newUser.Id);
            Assert.Equal(user.Code, newUser.Code);
            Assert.Equal(user.InstanceId, newUser.InstanceId);
            Assert.Equal(user.Surname, newUser.Surname);
            Assert.Equal(user.Name, newUser.Name);
            Assert.Equal(user.Role, newUser.Role);
        }

        [Fact]
        public async void AddTwoSameUsers()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            var secondUser = new User();
            secondUser.Deserialize(user.Serialize());
            await this.userManager.CreateAsync(user);
            var newSecondUserResult = await this.userManager.CreateAsync(secondUser);
            Assert.False(newSecondUserResult.Success);
        }
        
        [Theory]
        [InlineData(false, null, null, null, null, null)]
        [InlineData(true, "Petrov", "Ivan", null, null, null)]
        [InlineData(false, "Petrov", "Ivan", null, null, DefaultRoles.CreatorRole)]
        [InlineData(true, "Petrov", null, null, null, DefaultRoles.CreatorRole)]
        [InlineData(true, null, "Ivan", null, null, DefaultRoles.CreatorRole)]
        [InlineData(true, "", "Ivan", null, null, DefaultRoles.CreatorRole)]
        [InlineData(false, null, null, "4231", "123", null)]
        public async void AddUserWrongData(
            bool useInstanceId, 
            string surname, 
            string name, 
            string patronymic,
            string phoneNumber,
            string role)
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = useInstanceId ? this.instance.Id : Guid.Empty,
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                PhoneNumber = phoneNumber,
                Role = role,
            };
            
            // Act
            var result = await this.userManager.CreateAsync(user);
            
            // Assert
            Assert.False(result.Success);
        }
        
        [Fact]
        public async void EditUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            var newData = new User
            {
                Id = user.Id,
                InstanceId = this.instance.Id,
                Surname = "Modified_Petrov",
                Name = "Modified_Ivan",
                Patronymic = "Alexeevich",
                PhoneNumber = "9123",
            };
            
            // Act
            var result = await this.userManager.EditAsync(newData);
            
            // Arrange
            Assert.True(result.Success);
            Assert.Equal("Modified_Petrov", result.Result.Surname);
            Assert.Equal("Modified_Ivan", result.Result.Name);
            Assert.Equal("Alexeevich", result.Result.Patronymic);
            Assert.Equal("9123", result.Result.PhoneNumber);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void EditUserWrongData(byte mask)
        {
            var rightId = (mask & 1) != 0;
            var rightInstanceId = (mask & 2) != 0;
            
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            var newData = new User
            {
                Id = rightId ? user.Id : Guid.NewGuid(),
                InstanceId = rightInstanceId ? this.instance.Id : Guid.Empty,
                Surname = "New surname",
            };
            
            // Act
            var result = await this.userManager.EditAsync(newData);
            
            // Arrange
            Assert.False(result.Success);
        }

        [Fact]
        public async void GetUserById()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            var createUserResult = await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.GetAsync(user.Id, this.instance.Id);
            
            // Assert
            Assert.True(getUserResult.Success);
            Assert.Equal(createUserResult.Result.Id, getUserResult.Result.Id);
        }
        
        [Fact]
        public async void GetUserByCode()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            var createUserResult = await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.GetAsync(user.Code, this.instance.Id);
            
            // Assert
            Assert.True(getUserResult.Success);
            Assert.Equal(createUserResult.Result.Id, getUserResult.Result.Id);
        }
        
        [Fact]
        public async void GetUserId()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            var createUserResult = await this.userManager.CreateAsync(user);
            
            // Act
            var id = await this.userManager.GetIdAsync(user.Code, this.instance.Id);
            
            // Assert
            Assert.Equal(createUserResult.Result.Id, id);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetUserByIdWrongData(byte mask)
        {
            var rightId = (mask & 1) != 0;
            var rightInstanceId = (mask & 2) != 0;
            
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.GetAsync(
                rightId ? user.Id : Guid.NewGuid(), 
                rightInstanceId ? this.instance.Id : Guid.NewGuid());
            
            // Assert
            Assert.False(getUserResult.Success);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetUserByCodeWrongData(byte mask)
        {
            var rightCode = (mask & 1) != 0;
            var rightInstanceId = (mask & 2) != 0;
            
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.GetAsync(
                rightCode ? user.Code : Guid.NewGuid().ToString("N").Substring(0, 10), 
                rightInstanceId ? this.instance.Id : Guid.NewGuid());
            
            // Assert
            Assert.False(getUserResult.Success);
        }
        
        
        [Fact]
        public async void DeleteUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.DeleteAsync(user.Id, this.instance.Id);
            
            // Assert
            Assert.True(getUserResult.Success);
            var id = await this.userManager.GetIdAsync(user.Code, this.instance.Id);
            Assert.Null(id);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeleteUserWrongData(byte mask)
        {
            var rightId = (mask & 1) != 0;
            var rightInstanceId = (mask & 2) != 0;
            
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                InstanceId = this.instance.Id,
                Surname = "Petrov",
                Name = "Ivan",
                Role = DefaultRoles.CreatorRole,
            };
            await this.userManager.CreateAsync(user);
            
            // Act
            var getUserResult = await this.userManager.DeleteAsync(
                rightId ? user.Id : Guid.NewGuid(), 
                rightInstanceId ? this.instance.Id : Guid.NewGuid());
            
            // Assert
            Assert.False(getUserResult.Success);
            var id = await this.userManager.GetIdAsync(user.Code, this.instance.Id);
            Assert.NotNull(id);
        }
    }
}