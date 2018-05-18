using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Tasks.Routing;
using DeliveryTrackerScheduler.Common;
using Microsoft.AspNetCore.Routing;
using NLog;
using Quartz;

namespace DeliveryTrackerScheduler.Tasks
{
    public sealed class RouteBuilderJob : DeliveryTrackerJob
    {
        private readonly IRoutingService routingService;

        private readonly IPostgresConnectionProvider cp;
        
        public RouteBuilderJob(
            IRoutingService routingService,
            IPostgresConnectionProvider cp)
        {
            this.routingService = routingService;
            this.cp = cp;
        }

        protected override async Task ExecuteInternal(
            IJobExecutionContext context,
            Logger logger)
        {
            using (var conn = this.cp.Create().Connect())
            {
                var instances = new List<Guid>();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select id from instances";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            instances.Add(reader.GetGuid(0));
                        }
                    }
                }

                foreach (var instance in instances)
                {
                    await this.routingService.BuildDailyRoutesAsync(instance);
                }
            }
        }
    }
}