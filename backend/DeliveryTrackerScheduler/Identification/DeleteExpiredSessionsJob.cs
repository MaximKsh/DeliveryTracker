using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTrackerScheduler.Common;
using NLog;
using Quartz;

namespace DeliveryTrackerScheduler.Identification
{
    public sealed class DeleteExpiredSessionsJob : DeliveryTrackerJob
    {
        private readonly ISecurityManager securityManager;

        public DeleteExpiredSessionsJob(
            ISecurityManager securityManager)
        {
            this.securityManager = securityManager;
        }

        protected override async Task ExecuteInternal(
            IJobExecutionContext context,
            Logger logger)
        {
            await this.securityManager.DeleteAllExpiredAsync();
        }
    }
}