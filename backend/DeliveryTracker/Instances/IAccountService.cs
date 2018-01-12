using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    public interface IAccountService
    {
        Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(User user, UsernamePassword usernamePassword);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(UsernamePassword usernamePassword);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(UsernamePassword usernamePassword);

        Task<ServiceResult> EditAsync(string username, User newData);

        Task<ServiceResult> ValidatePasswordAsync(UsernamePassword usernamePassword);
        
        Task<ServiceResult> ChangePasswordAsync(
            string userName,
            string oldPassword,
            string newPassword);
    }
}