using System.Threading.Tasks;
using DeliveryTracker.Instances;
using DeliveryTrackerScheduler.Common;
using NLog;
using Quartz;

namespace DeliveryTrackerScheduler.Identification
{
    public sealed class DeleteExpiredInvitationsJob : DeliveryTrackerJob
    {

        private readonly IInvitationService invitationService;

        public DeleteExpiredInvitationsJob(
            IInvitationService invitationService)
        {
            this.invitationService = invitationService;
        }

        protected override async Task ExecuteInternal(
            IJobExecutionContext context,
            Logger logger)
        {
            await this.invitationService.DeleteAllExpiredAsync();
        }
    }
}