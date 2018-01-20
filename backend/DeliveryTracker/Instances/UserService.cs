using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Instances
{
    public class UserService : IUserService
    {
        #region fields

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserManager userManager;

        private readonly IUserCredentialsAccessor credentialsAccessor;
        
        #endregion
        
        #region constructor

        public UserService(
            IPostgresConnectionProvider cp,
            IUserManager userManager,
            IUserCredentialsAccessor credentialsAccessor)
        {
            this.cp = cp;
            this.userManager = userManager;
            this.credentialsAccessor = credentialsAccessor;
        }
        
        #endregion
        
        #region public
        
        public async Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.UserCredentials;
            if (credentials.Valid)
            {
                return await this.userManager.GetAsync(userId, credentials.InstanceId, oc);    
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        public async Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.UserCredentials;
            if (credentials.Valid)
            {
                return await this.userManager.GetAsync(code, credentials.InstanceId, oc);    
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        public async Task<ServiceResult<User>> EditAsync(User newData, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.UserCredentials;
            if (credentials.Valid)
            {
                newData.InstanceId = credentials.InstanceId;
                return await this.userManager.EditAsync(newData, oc);   
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        public async Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.UserCredentials;
            if (credentials.Valid)
            {
                return await this.userManager.DeleteAsync(userId, credentials.InstanceId, oc);  
            }
            return new ServiceResult(ErrorFactory.AccessDenied());
        }
        
        #endregion
    }
}