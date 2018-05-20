using System;
using System.Collections.Generic;
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

        Task<ServiceResult<List<Route>>> BuildRoutesAsync(
            List<TaskRouteItem> tasks,
            List<Guid> performers);
        
    }
}