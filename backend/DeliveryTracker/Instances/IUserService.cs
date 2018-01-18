using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IUserService
    {
        Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
               
        Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<User>> EditAsync(User newData, NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null);
    }
}