using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Identification
{
    public interface IDeviceManager
    {
        Task<ServiceResult> UpdateUserDeviceAsync(
            Device device,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<Device>> GetUserDeviceAsync(
            Guid userId,
            NpgsqlConnectionWrapper oc = null);
    }
}