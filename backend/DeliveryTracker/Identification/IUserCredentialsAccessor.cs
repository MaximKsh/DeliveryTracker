namespace DeliveryTracker.Identification
{
    public interface IUserCredentialsAccessor
    {
        UserCredentials UserCredentials { get; }
    }
}