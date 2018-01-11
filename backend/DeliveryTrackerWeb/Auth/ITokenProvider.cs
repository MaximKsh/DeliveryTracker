using DeliveryTracker.Instances;

namespace DeliveryTrackerWeb.Auth
{
    public interface ITokenProvider
    {
        string CreateToken(UserCredentials userCredentials);
    }
}