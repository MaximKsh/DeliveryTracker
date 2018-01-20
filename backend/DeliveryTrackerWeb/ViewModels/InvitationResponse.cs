using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InvitationResponse : ResponseBase
    {
        public InvitationResponse() : base()
        {
        }

        public InvitationResponse(IError error) : base(error)
        {
        }

        public InvitationResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public Invitation Invitation { get; set; }
        
    }
}