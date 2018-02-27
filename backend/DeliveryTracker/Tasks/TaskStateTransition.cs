using System;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskStateTransition
    {
        public Guid Id { get; set; }
        
        public Guid Role { get; set; }
        
        public Guid InitialState { get; set; }

        public Guid FinalState { get; set; }
        
        public string ButtonCaption { get; set; }
        
    }
}