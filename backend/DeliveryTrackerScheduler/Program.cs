using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Database;
using DeliveryTracker.Geopositioning;
using DeliveryTracker.Notifications;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using DeliveryTracker.Views;
using DeliveryTrackerScheduler.Common;
using DeliveryTrackerScheduler.Identification;
using DeliveryTrackerScheduler.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using static DeliveryTrackerScheduler.Common.SchedulerIdentites;
using Logger = NLog.Logger;
using LogLevel = NLog.LogLevel;


namespace DeliveryTrackerScheduler
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();
            var logger = LogManager.GetCurrentClassLogger();
            LogProvider.SetCurrentLogProvider(new NLogProvider(logger));

            try
            {
                await StartScheduler(serviceProvider, logger);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Fatal, e, "DeliveryTrackerScheduler exception");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static async Task StartScheduler(IServiceProvider serviceProvider, Logger logger)
        {
            try
            {
                var factory = new StdSchedulerFactory();
                var scheduler = await factory.GetScheduler();
                scheduler.JobFactory = new InjectableJobFactory(serviceProvider);
                await scheduler.Start();

                await SetJobs(scheduler);

                // SIGINT
                Console.CancelKeyPress += (
                    sender,
                    args) =>
                {
                    logger.Log(LogLevel.Info, "SIGINT");
                    scheduler.Shutdown(true).Wait();
                    LogManager.Shutdown();
                };
                // SIGTERM 
                AssemblyLoadContext.Default.Unloading += context =>
                {
                    logger.Log(LogLevel.Info, "SIGTERM");
                    scheduler.Shutdown(true).Wait();
                    LogManager.Shutdown();
                };
                await Task.Delay(Timeout.Infinite);
            }
            catch (SchedulerException se)
            {
                logger.Log(LogLevel.Fatal, se, "DeliveryTrackerScheduler exception");
            }
        }
        
        private static IServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var services = new ServiceCollection();
            services
                .AddDeliveryTrackerCommon()
                .AddDeliveryTrackerDatabase()
                .AddDeliveryTrackerIdentification(configuration)
                .AddDeliveryTrackerGeopositioning()
                .AddDeliveryTrackerInstances()
                .AddDeliveryTrackerReferences()
                .AddDeliveryTrackerViews()
                .AddDeliveryTrackerTasks()
                .AddDeliveryTrackerNotifications()
                
                
                // Замена для дефолтных зависимостей
                .AddSingleton<IDeliveryTrackerSerializer, DeliveryTrackerDefaultSerializer>()
                .AddSingleton<IUserCredentialsAccessor, SchedulerUserCredentialsAccessor>()
                
                // Регистрация джобов
                .AddIdentificationJobs()
                .AddTasksJobs()
                ;

            var provider = services.BuildServiceProvider();
            provider.GetService<ISettingsStorage>()
                .AddDeliveryTrackerDatabaseSettings(configuration)
                .AddDeliveryTrackerIdentificationSettings(configuration)
                .AddDeliveryTrackerInstancesSettings(configuration)
                .AddDeliveryTrackerNotificationSettings(configuration);

            return provider;
        }

        private static async Task SetJobs(IScheduler scheduler)
        {
            //await scheduler.ScheduleIdentificationJobs();
            await scheduler.ScheduleTasksJobs();
        }
        
    }
}