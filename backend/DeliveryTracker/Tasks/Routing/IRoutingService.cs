using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Tasks.Routing
{
    public interface IRoutingService
    {
        Task<ServiceResult> BuildDailyRoutesAsync(
            Guid instanceId,
            DateTime? date = null,
            NpgsqlConnectionWrapper oc = null);
    }
}