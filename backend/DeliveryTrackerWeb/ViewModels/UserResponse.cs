using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserResponse : ResponseBase
    {
        public UserResponse() : base()
        {
        }

        public UserResponse(IError error) : base(error)
        {
        }

        public UserResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
        
        public User User { get; set; }
    }
}