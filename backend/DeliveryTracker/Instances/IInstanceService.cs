using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public interface IInstanceService
    {
        Task<ServiceResult<Tuple<Instance, User, UserCredentials>>> CreateAsync(string instance,
            User creatorInfo,
            CodePassword codePassword,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Instance>> GetAsync(
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null);
    }
}