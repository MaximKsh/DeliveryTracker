using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Identification
{
    public class SecurityManagerTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IPostgresConnectionProvider provider;
        private readonly ISecurityManager defaultSecurityManager;
        private readonly Mock<IUserCredentialsAccessor> accessorMock;
        private readonly Instance defaultInstance;
        
        public SecurityManagerTest()
        {
            this.provider = new PostgresConnectionProvider(this.SettingsStorage);

            User me;
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
            }
            this.accessorMock = new Mock<IUserCredentialsAccessor>();
            this.accessorMock
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(me));
            
            this.defaultSecurityManager = new SecurityManager(
                this.provider, this.accessorMock.Object, this.SettingsStorage);
            
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
            }
        }

        [Fact]
        public async void SetPassword()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            
            // Act
            var result = await this.defaultSecurityManager.SetPasswordAsync(user.Id, "123");
            
            // Assert
            Assert.True(result.Success);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("````")]
        public async void SetPasswordInvalid(string pwd)
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            
            // Act
            var result = await this.defaultSecurityManager.SetPasswordAsync(user.Id, pwd);
            
            // Assert
            Assert.False(result.Success);
        }
        
        [Fact]
        public async void ValidatePasswordIdCorrect()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            await this.defaultSecurityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Act
            var result = await this.defaultSecurityManager.ValidatePasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Assert
            Assert.True(result.Success);
        }
        
        [Fact]
        public async void ValidatePasswordCodeCorrect()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            await this.defaultSecurityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Act
            var result = await this.defaultSecurityManager.ValidatePasswordAsync(user.Code, TestHelper.CorrectPassword);
            
            // Assert
            Assert.True(result.Success);
        }
        
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("````")]
        public async void ValidatePasswordIdWrong(string pwd)
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            await this.defaultSecurityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Act
            var result = await this.defaultSecurityManager.ValidatePasswordAsync(user.Id, pwd);
            
            // Assert
            Assert.False(result.Success);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("````")]
        public async void ValidatePasswordCodeWrong(string pwd)
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            await this.defaultSecurityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Act
            var result = await this.defaultSecurityManager.ValidatePasswordAsync(user.Code, pwd);
            
            // Assert
            Assert.False(result.Success);
        }
        
        [Fact]
        public async void NewSession()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            await this.defaultSecurityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            var result = await this.defaultSecurityManager.ValidatePasswordAsync(user.Id, TestHelper.CorrectPassword);
            
            // Act
            var newSessionResult = await this.defaultSecurityManager.NewSessionAsync(result.Result);
            
            // Assert
            Assert.True(newSessionResult.Success);
            Assert.False(string.IsNullOrWhiteSpace(newSessionResult.Result.SessionToken));
            Assert.False(string.IsNullOrWhiteSpace(newSessionResult.Result.RefreshToken));
        }
        
        [Fact]
        public async void RefreshSession()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(user));
            var securityManager = new SecurityManager(
                this.provider, accessor.Object, this.SettingsStorage);
            await securityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            var result = await securityManager.ValidatePasswordAsync(user.Id, TestHelper.CorrectPassword);
            var newSessionResult = await securityManager.NewSessionAsync(result.Result);
            
            // Act
            var refreshResult = await securityManager.RefreshSessionAsync(newSessionResult.Result.RefreshToken);
            
            // Assert
            Assert.True(refreshResult.Success);
            Assert.False(string.IsNullOrWhiteSpace(refreshResult.Result.SessionToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshResult.Result.RefreshToken));
        }
        
        [Fact]
        public async void ValidateAfterRefresh()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(user));
            var securityManager = new SecurityManager(
                this.provider, accessor.Object, this.SettingsStorage);
            await securityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            var result = await securityManager.ValidatePasswordAsync(user.Id, TestHelper.CorrectPassword);
            var newSessionResult = await securityManager.NewSessionAsync(result.Result);
            await securityManager.RefreshSessionAsync(newSessionResult.Result.RefreshToken);
            
            // Act
            // ReSharper disable once PossibleInvalidOperationException
            var hasSession = await securityManager.HasSession(user.Id, newSessionResult.Result.SessionTokenId.Value);
            
            // Assert
            Assert.False(hasSession.Success);
        }
        
        [Fact]
        public async void DoubleRefresh()
        {
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            var accessor = new Mock<IUserCredentialsAccessor>();
            accessor
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(user));
            var securityManager = new SecurityManager(
                this.provider, accessor.Object, this.SettingsStorage);
            await securityManager.SetPasswordAsync(user.Id, TestHelper.CorrectPassword);
            var result = await securityManager.ValidatePasswordAsync(user.Id, TestHelper.CorrectPassword);
            var newSessionResult = await securityManager.NewSessionAsync(result.Result);
            await securityManager.RefreshSessionAsync(newSessionResult.Result.RefreshToken);
            
            // Act
            var refreshResult = await securityManager.RefreshSessionAsync(newSessionResult.Result.RefreshToken);
            
            // Assert
            Assert.False(refreshResult.Success);
        }

        [Theory]
        [InlineData(5, 10, false, false, false, "abc", 10, "aaa")]
        [InlineData(3, 5, false, false, false, "abc", 10, "abcabcabc")]
        [InlineData(3, 10, true, false, false, "abc", 10, "abcabcabc")]
        [InlineData(3, 10, false, true, false, "abcABC", 10, "ABCABCABC")]
        [InlineData(3, 10, true, true, false, "abcABC", 10, "ABCABCABC")]
        [InlineData(3, 10, true, true, false, "abc", 10, "abcabcabc")]
        [InlineData(3, 10, true, true, false, "1235", 10, "1235321")]
        [InlineData(3, 10, false, false, true, "abcABC", 10, "abcaAbc")]
        public async void SetPasswordInvalidPassword(
            int min,
            int max,
            bool upper,
            bool lower,
            bool digit,
            string alphabet,
            int sameCharactersInRow,
            string password
            )
        {
            var ps = new PasswordSettings(SettingsName.Password, min, max, upper, lower, digit, alphabet, sameCharactersInRow);
            var settingsStorage = new SettingsStorage()
                .RegisterSettings(ps)
                .RegisterSettings(this.DefaultTokenSettings);
            var securityManager = new SecurityManager(
                this.provider, this.accessorMock.Object, settingsStorage);
            // Arrange
            User user;
            using (var conn = this.provider.Create())
            {
                conn.Connect();
                user = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
            }
            
            // Act
            var result = await securityManager.SetPasswordAsync(user.Id, password);
            
            // Assert
            Assert.False(result.Success);
            Assert.All(result.Errors, e => Assert.Equal(ErrorCode.IncorrectPassword, e.Code));
        }

    }
}