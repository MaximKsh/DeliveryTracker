using System;
using System.Collections.Generic;

namespace DeliveryTrackerWeb.ViewModels
{
    public class ReferenceRequest : RequestBase
    {
        public Guid Id { get; set; }
        
        public IDictionary<string, object> Entity { get; set; }
    }
}