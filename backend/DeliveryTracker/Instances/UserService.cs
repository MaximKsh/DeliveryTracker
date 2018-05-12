using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <inheritdoc />
    public sealed class UserService : IUserService
    {
        #region sql

        private const string SqlGetUserRole = @"
select role
from users
where id = @id
;
";

        #endregion
        
        #region fields

        private readonly IUserManager userManager;

        private readonly IUserCredentialsAccessor credentialsAccessor;

        private readonly IPostgresConnectionProvider cp;
        
        #endregion
        
        #region constructor

        public UserService(
            IUserManager userManager,
            IUserCredentialsAccessor credentialsAccessor,
            IPostgresConnectionProvider cp)
        {
            this.userManager = userManager;
            this.credentialsAccessor = credentialsAccessor;
            this.cp = cp;
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
            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var userRole = await GetUserRole(newData.Id, conn);
                var credentials = this.credentialsAccessor.GetUserCredentials();
                if (credentials.Valid && CanEdit(credentials, userRole))
                {
                    newData.InstanceId = credentials.InstanceId;
                    return await this.userManager.EditAsync(newData, conn);   
                }
                return new ServiceResult<User>(ErrorFactory.AccessDenied());   
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var userRole = await GetUserRole(userId, conn);
                if (userRole == DefaultRoles.CreatorRole)
                {
                    return new ServiceResult(ErrorFactory.CantDeleteCreator());
                }
                
                var credentials = this.credentialsAccessor.GetUserCredentials();
                if (credentials.Valid && CanDelete(credentials, userRole))
                {
                    return await this.userManager.DeleteAsync(userId, credentials.InstanceId, conn);  
                }
                return new ServiceResult(ErrorFactory.CantDeleteUser());
            }
        }
        
        #endregion

        #region private

        private static bool CanEdit(
            UserCredentials cred,
            Guid roleID) =>
            ((roleID == DefaultRoles.CreatorRole || roleID == DefaultRoles.ManagerRole) && cred.Role == DefaultRoles.CreatorRole)
            || (roleID == DefaultRoles.PerformerRole && (cred.Role == DefaultRoles.CreatorRole || cred.Role == DefaultRoles.ManagerRole));

        private static bool CanDelete(
            UserCredentials cred,
            Guid roleID) =>
            roleID == DefaultRoles.ManagerRole && cred.Role == DefaultRoles.CreatorRole
            || (roleID == DefaultRoles.PerformerRole && (cred.Role == DefaultRoles.CreatorRole || cred.Role == DefaultRoles.ManagerRole));


        private static async Task<Guid> GetUserRole(
            Guid userId,
            NpgsqlConnectionWrapper conn)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SqlGetUserRole;
                command.Parameters.Add(new NpgsqlParameter("id", userId));
                return (Guid) await command.ExecuteScalarAsync();
            }
        }
        
        #endregion
    }
}