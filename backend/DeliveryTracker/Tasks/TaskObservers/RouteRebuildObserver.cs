using System.Threading.Tasks;
using DeliveryTracker.Tasks.Routing;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public sealed class RouteRebuildObserver: TaskObserverBase
    {
        private readonly IRoutingService routingService;

        public RouteRebuildObserver(
            IRoutingService routingService)
        {
            this.routingService = routingService;
        }

        public override async Task HandleEditTask(
            ITaskObserverContext ctx)
        {
            if (ctx.TaskChanges.ClientAddressId != null
                || ctx.TaskChanges.PerformerId != null
                || ctx.TaskChanges.DeliveryFrom != null
                || ctx.TaskChanges.DeliveryTo != null)
            {
                await this.routingService.BuildDailyRoutesAsync(ctx.Credentials.InstanceId, null, true, ctx.ConnectionWrapper);
            }

            await Task.CompletedTask;
        }

        public override async Task HandleTransition(
            ITaskObserverContext ctx)
        {
            if (ctx.Transition.FinalState == DefaultTaskStates.Queue.Id)
            {
                await this.routingService.BuildDailyRoutesAsync(ctx.Credentials.InstanceId, null, true, ctx.ConnectionWrapper);
            }
        }
        
    }
}