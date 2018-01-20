using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class InvitationRequest : RequestBase
    {
        public User User { get; set; }
        
        public string Code { get; set; }
        
    }
}