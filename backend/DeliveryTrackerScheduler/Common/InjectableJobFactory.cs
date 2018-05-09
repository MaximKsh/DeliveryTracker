using System;
using NLog;
using Quartz;
using Quartz.Spi;

namespace DeliveryTrackerScheduler.Common
{
    public sealed class InjectableJobFactory : IJobFactory
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider serviceProvider;

        public InjectableJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;
                Logger.Debug($"Producing instance of Job '{jobDetail.Key}', class={jobType.FullName}");

                return this.serviceProvider.GetService(jobType) as IJob;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, nameof(IJobFactory.NewJob));
                throw new SchedulerException($"Problem instantiating class '{bundle.JobDetail.JobType.FullName}'", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}