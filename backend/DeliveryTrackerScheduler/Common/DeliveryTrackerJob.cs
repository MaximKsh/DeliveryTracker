using System;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace DeliveryTrackerScheduler.Common
{
    public abstract class DeliveryTrackerJob : IJob
    {
        protected abstract Task ExecuteInternal(
            IJobExecutionContext context,
            Logger logger);
        
        public async Task Execute(
            IJobExecutionContext context)
        {
            var currentTypeName = this.GetType().Name;
            var logger = LogManager.GetLogger(currentTypeName);
            try
            {
                logger.Log(LogLevel.Trace, "Start.");
                await Task.Run(async () => { await this.ExecuteInternal(context, logger); });
                logger.Log(LogLevel.Trace, "Successfuly complete.");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e, "Exception");
            }
        }
    }
}