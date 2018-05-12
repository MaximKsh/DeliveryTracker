using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Notifications;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Instances
{
    public class InvitationServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IInvitationService defaultInvitationService;

        private readonly Instance defaultInstance;
        private readonly User creator;
        private readonly User manager;
        private readonly User performer;
        
        private readonly Mock<IUserCredentialsAccessor> creatorCredentialsMock = new Mock<IUserCredentialsAccessor>();
        private readonly Mock<IUserCredentialsAccessor> managerCredentialsMock = new Mock<IUserCredentialsAccessor>();
        private readonly Mock<IUserCredentialsAccessor> performerCredentialsMock = new Mock<IUserCredentialsAccessor>();
        private readonly IList<IUserCredentialsAccessor> accessors;
        
        
        public InvitationServiceTest()
        {
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                this.creator = TestHelper.CreateRandomUser(DefaultRoles.CreatorRole, this.defaultInstance.Id, conn);
                this.manager = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                this.performer = TestHelper.CreateRandomUser(DefaultRoles.PerformerRole, this.defaultInstance.Id, conn);
            }

            this.accessors = new List<IUserCredentialsAccessor>
            {
                this.creatorCredentialsMock.Object,
                this.managerCredentialsMock.Object,
                this.performerCredentialsMock.Object,
            };
            
            this.creatorCredentialsMock
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(this.creator));
            this.managerCredentialsMock
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(this.manager));
            this.performerCredentialsMock
                .Setup(x => x.GetUserCredentials())
                .Returns(new UserCredentials(this.performer));
            
            
            this.defaultInvitationService = new InvitationService(
                this.Cp, 
                this.SettingsStorage, 
                this.creatorCredentialsMock.Object,
                new Mock<INotificationService>().Object,
                null);
        }

        [Fact]
        public async void GenerateWithChecking()
        {
            // Act
            var result = await this.defaultInvitationService.GenerateUniqueCodeAsync();
            
            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        private const string Numerics = "0123456789";
        private const string Lower = "qwertyuiopasdfghjklzxcvbnm";
        private const string Upper = "QWERTYUIOPASDFGHJKLZXCVBNM";
        private const string ManyDifferentCharacters = "`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./QWERTYUIOP[]ASDFGHJKL;'ZXCVBNM,./";
        
        [Theory, PairwiseData]
        public void TestInvitationSettings(
            [CombinatorialValues(1, 2, 5, 8, 10, 15, 20)]int codeLength,
            [CombinatorialValues(Numerics, Lower, Upper, ManyDifferentCharacters)] string alphabet)
        {
            // Arrange
            var invitationSettings = new InvitationSettings(
                SettingsName.Invitation,
                1,
                codeLength,
                alphabet);
            var settingsStorage = new SettingsStorage()
                .RegisterSettings(invitationSettings);
            var invitationService = new InvitationService(this.Cp, settingsStorage, null,
                new Mock<INotificationService>().Object, null);

            // Act
            var code = invitationService.GenerateCode();
            
            // Assert
            Assert.Equal(codeLength, code.Length);
            Assert.All(code, c => Assert.Contains(alphabet, ac => c == ac));
            
        }

        public static IEnumerable<object[]> DataForCreateInvitation()
        {
            yield return new object[]
            {
                0,
                DefaultRoles.ManagerRole
            };
            yield return new object[]
            {
                0,
                DefaultRoles.PerformerRole
            };
            yield return new object[]
            {
                1,
                DefaultRoles.PerformerRole
            };
        }
        
        [Theory]
        [MemberData(nameof(DataForCreateInvitation))]
        public async void CreateInvitation(int accesorIndex, Guid role)
        {
            // Arrange
            var accesor = this.accessors[accesorIndex];
            var invitationService = new InvitationService(this.Cp, this.SettingsStorage, accesor,
                new Mock<INotificationService>().Object, null);
            var userData = new User
            {
                Role = role,
            };
            
            // Act
            var result = await invitationService.CreateAsync(userData);
            
            // Arrange
            Assert.True(result.Success);
        }
        
        public static IEnumerable<object[]> DataForCreateInvitationAccessError()
        {
            yield return new object[]
            {
                0,
                DefaultRoles.CreatorRole
            };
            yield return new object[]
            {
                1,
                DefaultRoles.CreatorRole
            };
            yield return new object[]
            {
                2,
                DefaultRoles.CreatorRole
            };
            yield return new object[]
            {
                2,
                DefaultRoles.ManagerRole
            };
            yield return new object[]
            {
                2,
                DefaultRoles.PerformerRole
            };
        }
        
        [Theory]
        [MemberData(nameof(DataForCreateInvitationAccessError))]
        public async void CreateInvitationAccessError(int accesorIndex, Guid role)
        {
            // Arrange
            var accesor = this.accessors[accesorIndex];
            var invitationService = new InvitationService(this.Cp, this.SettingsStorage, accesor,
                new Mock<INotificationService>().Object, null);
            var userData = new User
            {
                Role = role,
            };
            
            // Act
            var result = await invitationService.CreateAsync(userData);
            
            // Arrange
            Assert.False(result.Success);
            Assert.Contains(ErrorFactory.AccessDenied(), result.Errors);
        }

        [Fact]
        public async void GetInvitation()
        {
            // Arrange
            var userData = new User
            {
                Role = DefaultRoles.PerformerRole,
            };
            var result = await this.defaultInvitationService.CreateAsync(userData);
            
            // Act
            var getResult = await this.defaultInvitationService.GetAsync(result.Result.InvitationCode);
            
            // Arrange
            Assert.True(getResult.Success);
        }
        
        [Fact]
        public async void GetInvitationNotFound()
        {
            // Act
            var getResult = await this.defaultInvitationService.GetAsync(Guid.NewGuid().ToString().Substring(0, 10));
            
            // Arrange
            Assert.False(getResult.Success);
        }
        
        [Fact]
        public async void DeleteInvitation()
        {
            // Arrange
            var userData = new User
            {
                Role = DefaultRoles.PerformerRole,
            };
            var result = await this.defaultInvitationService.CreateAsync(userData);
            
            // Act
            var deleteResult = await this.defaultInvitationService.DeleteAsync(result.Result.InvitationCode);
            
            // Arrange
            Assert.True(deleteResult.Success);
            var getResult = await this.defaultInvitationService.GetAsync(result.Result.InvitationCode);
            Assert.False(getResult.Success);
        }
    }
}