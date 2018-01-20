using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Identification;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class AccountRequest : RequestBase
    {
        public User User { get; set; }
        
        public CodePassword CodePassword { get; set; }
        
        public CodePassword NewCodePassword { get; set; }
    }
}