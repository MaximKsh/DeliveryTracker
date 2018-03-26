using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class TransitionObserverContext : ITransitionObserverContext
    {
        public TransitionObserverContext(
            TaskInfo taskInfo,
            UserCredentials credentials,
            TaskStateTransition transition,
            NpgsqlConnectionWrapper connectionWrapper)
        {
            this.TaskInfo = taskInfo;
            this.Credentials = credentials;
            this.Transition = transition;
            this.ConnectionWrapper = connectionWrapper;
        }

        public TaskInfo TaskInfo { get; }
        public TaskStateTransition Transition { get; }
        public UserCredentials Credentials { get; }
        public NpgsqlConnectionWrapper ConnectionWrapper { get; }
    }
}