using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public sealed class AccountServiceResult
    {
        public User User { get; set; }
        
        public UserCredentials Credentials { get; set; }
    }
}