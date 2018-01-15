using System;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Xunit;

namespace DeliveryTracker.Tests.Identification
{
    public class UserManagerTest : DeliveryTrackerTestBase
    {
        private readonly IPostgresConnectionProvider provider;

        private readonly IUserManager userManager;
        
        /// <summary>
        /// Инстанс, для которого тестируются пользователи.
        /// </summary>
        private readonly Instance instance;
        
        public UserManagerTest() : base()
        {
            this.provider = new PostgresConnectionProvider(this.Configuration);
            this.userManager = new UserManager(this.provider);
            using (var conn = this.provider.Create())
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
        [InlineData(false, null, null, "4231", "123", null)]
        public async void AddUserWrongData(
            bool useInstanceId, 
            string surname, 
            string name, 
            string patronymic,
            string phoneNumber,
            string role)
        {
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
            var result = await this.userManager.CreateAsync(user);
            Assert.False(result.Success);
        }
    }
}