using System;
using DeliveryTracker.Identification;

namespace DeliveryTrackerWeb.ViewModels
{
    public class UserRequest : RequestBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public User User { get; set; }
    }
}