using System;
using Quartz.Logging;

namespace DeliveryTrackerScheduler.Common
{
    public sealed class NLogProvider: ILogProvider
    {
        private readonly NLog.Logger logger;

        public NLogProvider(
            NLog.Logger logger)
        {
            this.logger = logger;
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                var nlogLevel = ConvertLogLevel(level);

                if (exception != null)
                {
                    this.logger.Log(nlogLevel, exception, "DeliveryTrackerScheduler exception");
                }
                if (func != null)
                {
                    this.logger.Log(nlogLevel, func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotSupportedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotSupportedException();
        }

        private static NLog.LogLevel ConvertLogLevel(
            LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Warn:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}