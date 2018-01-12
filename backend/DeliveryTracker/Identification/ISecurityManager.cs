using System;
using System.Threading.Tasks;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface ISecurityManager
    {
        Task<UserCredentials> ValidatePasswordAsync(
            string code,
            string password, 
            NpgsqlConnectionWrapper outerConnection = null);

        Task<bool> SetPasswordAsync(
            Guid userId, 
            string newPassword,
            NpgsqlConnectionWrapper outerConnection = null);

        string AcquireToken(UserCredentials credentials);
    }
}