using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class GeopositioningResponse : ResponseBase
    {
        public GeopositioningResponse() : base()
        {
        }

        public GeopositioningResponse(IError error) : base(error)
        {
        }

        public GeopositioningResponse(IReadOnlyList<IError> errors) : base(errors)
        {
        }
    }
}