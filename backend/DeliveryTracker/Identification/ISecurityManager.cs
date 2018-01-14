using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface ISecurityManager
    {
        Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            string code,
            string password, 
            NpgsqlConnectionWrapper outerConnection = null);

        Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            Guid userId,
            string password, 
            NpgsqlConnectionWrapper outerConnection = null);
        
        Task<ServiceResult<UserCredentials>> SetPasswordAsync(
            Guid userId, 
            string newPassword,
            NpgsqlConnectionWrapper outerConnection = null);

        string AcquireToken(UserCredentials credentials);
    }
}