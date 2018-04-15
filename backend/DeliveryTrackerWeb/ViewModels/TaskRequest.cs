using System;
using DeliveryTracker.Tasks;

namespace DeliveryTrackerWeb.ViewModels
{
    public class TaskRequest : RequestBase
    {
        public Guid Id { get; set; }
        
        public Guid TransitionId { get; set; }
        
        public TaskPackage TaskPackage { get; set; }
    }
}