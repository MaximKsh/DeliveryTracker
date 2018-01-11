using DeliveryTracker.Common;
using DeliveryTracker.Services;

namespace DeliveryTracker.Instances
{
    public interface IAccountService
    {
        ServiceResult<UserCredentials> Login(LoginPassword loginPassword);

        ServiceResult<User> About(string userName);

        ServiceResult Edit(string userName, User newData);

        ServiceResult ValidatePassword(LoginPassword loginPassword);
        
        ServiceResult ChangePassword(LoginPassword loginPassword);
    }
}