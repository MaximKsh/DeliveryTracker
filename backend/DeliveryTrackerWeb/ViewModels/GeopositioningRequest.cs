using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Geopositioning;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class GeopositioningRequest : RequestBase
    {
        public Geoposition Geoposition { get; set; }
    }
}