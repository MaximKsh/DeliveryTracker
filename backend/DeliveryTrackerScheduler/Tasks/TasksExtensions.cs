using System.Threading.Tasks;
using DeliveryTrackerScheduler.Common;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DeliveryTrackerScheduler.Tasks
{
    public static class TasksExtensions
    {
        public static IServiceCollection AddTasksJobs(
            this IServiceCollection collection)
        {
            collection
                .AddSingleton<RouteBuilderJob>()
                ;

            return collection;
        }
        
        public static async Task<IScheduler> ScheduleTasksJobs(
            this IScheduler scheduler)
        {
            var invitationJob = JobBuilder.Create<RouteBuilderJob>()
                .WithIdentity(SchedulerIdentites.Jobs.RouteBuilder, SchedulerIdentites.Groups.Default)
                .Build();
            var invitationTrigger = TriggerBuilder.Create()
                .WithIdentity(SchedulerIdentites.Triggers.RouteBuilder, SchedulerIdentites.Groups.Default)
                .StartNow()
                //.WithSimpleSchedule(x => x.WithIntervalInSeconds(6000).RepeatForever())
                .WithCronSchedule("0 0 3 * * ?")
                .Build();
            await scheduler.ScheduleJob(invitationJob, invitationTrigger);
            
            return scheduler;
        }
    }
}