using System;
using System.Collections.Generic;

namespace DeliveryTracker.Tasks.Routing
{
    public sealed class Route
    {
        public List<int> TaskRoute { get; set; }
        
        public List<int> Eta { get; set; }
        
        public Guid PerformerId { get; set; }
    }
}