using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IAccountService
    {
        Task<ServiceResult<Tuple<User, UserCredentials>>> RegisterAsync(
            User userInfo, 
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Tuple<User, UserCredentials>>> LoginWithRegistrationAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult> EditAsync(
            Guid userId,
            User newData,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult> ValidatePasswordAsync(
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult> ChangePasswordAsync(
            Guid userId,
            string oldPassword,
            string newPassword,
            NpgsqlConnectionWrapper oc = null);
    }
}