using DeliveryTracker.Identification;
using DeliveryTracker.Tasks;
using Xunit;

namespace DeliveryTracker.Tests.Tasks
{
    public sealed class TaskStateTransitionManagerTest : DeliveryTrackerConnectionTestBase
    {

        private readonly ITaskStateTransitionManager manager;
        
        public TaskStateTransitionManagerTest()
        {
            this.manager = new TaskStateTransitionManager(this.Cp);
        }

        [Fact]
        public async void GetTransition()
        {
            // Act
            var result = await this.manager.GetTransition(
                DefaultRoles.ManagerRole,
                DefaultTaskStates.Preparing.Id,
                DefaultTaskStates.IntoWork.Id);

            // Assert
            Assert.True(result.Success);
        }
        
        [Fact]
        public async void GetTransitionNotFound()
        {
            // Act
            var result = await this.manager.GetTransition(
                DefaultRoles.ManagerRole,
                DefaultTaskStates.Preparing.Id,
                DefaultTaskStates.Complete.Id);

            // Assert
            Assert.False(result.Success);
        }
        
    }
}