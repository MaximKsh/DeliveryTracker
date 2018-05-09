using System.Collections.Generic;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public class TaskObserverContext : ITaskObserverContext
    {
        public TaskObserverContext(
            TaskInfo taskChanges,
            TaskInfo taskInfo,
            UserCredentials credentials,
            TaskStateTransition transition,
            NpgsqlConnectionWrapper connectionWrapper)
        {
            this.TaskChanges = taskChanges;
            this.TaskInfo = taskInfo;
            this.Credentials = credentials;
            this.Transition = transition;
            this.ConnectionWrapper = connectionWrapper;
        }

        public TaskInfo TaskChanges { get; }
        public TaskInfo TaskInfo { get; }
        public TaskStateTransition Transition { get; }
        public UserCredentials Credentials { get; }
        public NpgsqlConnectionWrapper ConnectionWrapper { get; }
        public bool Cancel { get; set; } = false;
        public IList<IError> Errors { get; } = new List<IError>();
    }
}