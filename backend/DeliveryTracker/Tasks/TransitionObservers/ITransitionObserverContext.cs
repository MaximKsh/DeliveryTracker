using System;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public interface ITransitionObserverContext
    {
        TaskInfo TaskInfo { get; }

        TaskStateTransition Transition { get; }
        
        UserCredentials Credentials { get; }
    }
}