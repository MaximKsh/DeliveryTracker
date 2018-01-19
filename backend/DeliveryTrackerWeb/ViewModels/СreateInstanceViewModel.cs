using DeliveryTracker.Identification;

namespace DeliveryTrackerWeb.ViewModels
{
    public class CreateInstanceViewModel
    {
        public string InstanceName { get; set; }
        public User Creator { get; set; }
        public CodePassword CreatorCodePassword { get; set; }
    }
}