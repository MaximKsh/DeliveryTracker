using DeliveryTracker.Identification;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class TransitionObserverContext : ITransitionObserverContext
    {
        public TransitionObserverContext(
            TaskInfo taskInfo,
            UserCredentials credentials,
            TaskStateTransition transition)
        {
            this.TaskInfo = taskInfo;
            this.Credentials = credentials;
            this.Transition = transition;
        }

        public TaskInfo TaskInfo { get; }
        public TaskStateTransition Transition { get; }
        public UserCredentials Credentials { get; }
    }
}