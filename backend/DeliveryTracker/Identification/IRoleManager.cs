using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface IRoleManager
    {
        Task<ServiceResult<Role>> CreateAsync(
            string name,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Role>> ChangeNameAsync(
            Guid roleId,
            string name,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Role>> GetAsync(
            Guid roleId,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult> DeleteAsync(
            Guid roleId,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult> AddToRoleAsync(
            Guid userId,
            Guid roleId,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<IReadOnlyList<Role>>> GetUserRolesAsync(
            Guid userId, 
            int limit = DatabaseHelper.DefaultLimit,
            int offset = DatabaseHelper.DefaultOffset,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<IReadOnlyList<User>>> GetUsersInRoleAsync(
            Guid roleId,
            Guid instanceId,
            int limit = DatabaseHelper.DefaultLimit,
            int offset = DatabaseHelper.DefaultOffset,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult> RemoveFromRoleAsync(
            Guid userId, 
            Guid roleId, 
            NpgsqlConnectionWrapper oc = null);
    }
}