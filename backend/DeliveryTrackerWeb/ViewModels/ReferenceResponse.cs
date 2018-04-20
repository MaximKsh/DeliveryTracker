using System.Collections.Generic;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
// ReSharper disable UnusedAutoPropertyAccessor.Global

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
        
        public IReadOnlyDictionary<string, ReferenceDescription> ReferencesList { get; set; }
        
    }
}