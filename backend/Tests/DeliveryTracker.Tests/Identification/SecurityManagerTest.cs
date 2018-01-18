using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Xunit;

namespace DeliveryTracker.Tests.Identification
{
    public class SecurityManagerTest : DeliveryTrackerTestBase
    {
        private readonly IPostgresConnectionProvider provider;
        private readonly ISecurityManager defaultSecurityManager;
        private readonly Instance defaultInstance;
        
        public SecurityManagerTest()
        {
            this.provider = new PostgresConnectionProvider(this.Configuration);
            
            this.defaultSecurityManager = new SecurityManager(
                this.provider, this.DefaultTokenSettings, this.DefaultPasswordSettings);
            
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
        public async void AcquireToken()
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
            var token = this.defaultSecurityManager.AcquireToken(result.Result);
            
            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
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
        [InlineData(3, 10, false, false, false, "abc", 2, "aaabc")]
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
            var ps = new PasswordSettings(min, max, upper, lower, digit, alphabet, sameCharactersInRow);
            var securityManager = new SecurityManager(
                this.provider, this.DefaultTokenSettings, ps);
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