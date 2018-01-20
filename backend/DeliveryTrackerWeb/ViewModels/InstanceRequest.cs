using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class InstanceRequest : RequestBase
    {
        public Instance Instance { get; set; }
        
        public User Creator { get; set; }
        
        public CodePassword CodePassword{ get; set; }
    }
}