using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public sealed class InstanceServiceResult
    {
        
        public Instance Instance { get; set; }
        
        public UserCredentials Credentials { get; set; }

        public User User { get; set; }

    }
}