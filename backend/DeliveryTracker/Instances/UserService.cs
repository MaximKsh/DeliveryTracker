using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class UserService : IUserService
    {
        #region fields

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserManager userManager;

        private readonly IUserCredentialsAccessor credentials;
        
        #endregion
        
        #region constructor

        public UserService(
            IPostgresConnectionProvider cp,
            IUserManager userManager,
            IUserCredentialsAccessor credentials)
        {
            this.cp = cp;
            this.userManager = userManager;
            this.credentials = credentials;
        }
        
        #endregion
        
        #region public
        
        public async Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentials.UserCredentials;
            return await this.userManager.GetAsync(userId, oc);
        }

        public async Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null)
        {
            return await this.userManager.GetAsync(code, oc);
        }

        public async Task<ServiceResult<User>> EditAsync(User newData, NpgsqlConnectionWrapper oc = null)
        {
            return await this.userManager.EditAsync(newData, oc);
        }

        public async Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            return await this.userManager.DeleteAsync(userId, oc);
        }

        public async Task<ServiceResult> UpdatePositionAsync(
            Guid userId, 
            Geoposition position, 
            NpgsqlConnectionWrapper oc = null)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}