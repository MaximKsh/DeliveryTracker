using System.Collections.Generic;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    public class ReferenceResponse : ResponseBase
    { 
        public ReferenceResponse() : base()
        {
        }

        public ReferenceResponse(IError error) : base(error)
        {
        }

        public ReferenceResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public IDictionary<string, object> Entity { get; set; }
        
        public string[] ReferencesList { get; set; }
        
    }
}