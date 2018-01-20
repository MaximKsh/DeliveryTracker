using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InstanceResponse : ResponseBase
    {
        public InstanceResponse() : base()
        {
        }

        public InstanceResponse(IError error) : base(error)
        {
        }

        public InstanceResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public Instance Instance { get; set; }
        
        public User Creator { get; set; }
        
        public string Token { get; set; }
    }
}