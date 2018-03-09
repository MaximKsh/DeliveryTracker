using System;
using DeliveryTracker.Tasks;

namespace DeliveryTrackerWeb.ViewModels
{
    public class TaskRequest : RequestBase
    {
        public Guid Id { get; set; }
        
        public TaskInfo TaskInfo { get; set; }
    }
}