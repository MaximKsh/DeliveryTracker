using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <inheritdoc />
    public sealed class UserService : IUserService
    {
        #region fields

        private readonly IUserManager userManager;

        private readonly IUserCredentialsAccessor credentialsAccessor;
        
        #endregion
        
        #region constructor

        public UserService(
            IUserManager userManager,
            IUserCredentialsAccessor credentialsAccessor)
        {
            this.userManager = userManager;
            this.credentialsAccessor = credentialsAccessor;
        }
        
        #endregion
        
        #region public
        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.GetUserCredentials();
            if (credentials.Valid)
            {
                return await this.userManager.GetAsync(userId, credentials.InstanceId, oc: oc);    
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.GetUserCredentials();
            if (credentials.Valid)
            {
                return await this.userManager.GetAsync(code, credentials.InstanceId, oc);    
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        /// <inheritdoc />
        public async Task<ServiceResult<User>> EditAsync(User newData, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.credentialsAccessor.GetUserCredentials();
            
            // TODO: Изменять менеджеров может только создатель
            
            if (credentials.Valid)
            {
                newData.InstanceId = credentials.InstanceId;
                return await this.userManager.EditAsync(newData, oc);   
            }
            return new ServiceResult<User>(ErrorFactory.AccessDenied());
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            // TODO: Удалять менеджеров может только создатель
            
            
            var credentials = this.credentialsAccessor.GetUserCredentials();
            if (credentials.Valid)
            {
                return await this.userManager.DeleteAsync(userId, credentials.InstanceId, oc);  
            }
            return new ServiceResult(ErrorFactory.AccessDenied());
        }
        
        #endregion
    }
}