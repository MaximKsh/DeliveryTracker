using System.Threading.Tasks;
using DeliveryTrackerScheduler.Common;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DeliveryTrackerScheduler.Identification
{
    public static class IdentificationExtensions
    {

        public static IServiceCollection AddIdentificationJobs(
            this IServiceCollection collection)
        {
            collection
                .AddSingleton<DeleteExpiredInvitationsJob>()
                .AddSingleton<DeleteExpiredSessionsJob>();

            return collection;
        }
        
        public static async Task<IScheduler> ScheduleIdentificationJobs(
            this IScheduler scheduler)
        {
            var invitationJob = JobBuilder.Create<DeleteExpiredInvitationsJob>()
                .WithIdentity(SchedulerIdentites.Jobs.ExpiredInvitations, SchedulerIdentites.Groups.Default)
                .Build();
            var invitationTrigger = TriggerBuilder.Create()
                .WithIdentity(SchedulerIdentites.Triggers.ExpiredInvitations, SchedulerIdentites.Groups.Default)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
                //.WithCronSchedule("0 0 12 * * ?")
                .Build();
            await scheduler.ScheduleJob(invitationJob, invitationTrigger);
            
            var sessionJob = JobBuilder.Create<DeleteExpiredSessionsJob>()
                .WithIdentity(SchedulerIdentites.Jobs.ExpiredSessions, SchedulerIdentites.Groups.Default)
                .Build();
            var sessionTrigger = TriggerBuilder.Create()
                .WithIdentity(SchedulerIdentites.Triggers.ExpiredSessions, SchedulerIdentites.Groups.Default)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
                //.WithCronSchedule("0 0 12 * * ?")
                .Build();
            await scheduler.ScheduleJob(sessionJob, sessionTrigger);

            return scheduler;
        }
    }
}