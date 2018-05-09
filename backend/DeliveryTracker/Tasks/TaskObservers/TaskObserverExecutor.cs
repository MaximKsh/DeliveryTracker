using System;
using System.Threading.Tasks;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public sealed class TaskObserverExecutor : ITaskObserverExecutor
    {
        #region fields

        private readonly IServiceProvider provider;

       private readonly ILogger<TaskObserverExecutor> logger;
        
        #endregion
        
        #region constuctor

        public TaskObserverExecutor(
            IServiceProvider provider,
            ILogger<TaskObserverExecutor> logger)
        {
            this.provider = provider;
            this.logger = logger;
        }
        
        #endregion
        
        #region implementation
        
        public async Task ExecuteNew(
            ITaskObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITaskObserver>();

            foreach (var observer in observers)
            { 
                try
                {
                    await observer.HandleNewTask(ctx);
                }
                catch (Exception e)
                {
                    this.logger?.LogError($"{observer.GetType().Name} throws an exception when handling creation:{Environment.NewLine}" +
                                          $"{e.Message}{Environment.NewLine}" +
                                          $"{e.StackTrace}");

                    ctx.Cancel = true;
                }

                if (ctx.Cancel)
                {
                    ctx.Errors.Add(ErrorFactory.ObserverCancelExecution(observer));
                    return;
                }
                
            }
        }
        
        public async Task ExecuteEdit(
            ITaskObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITaskObserver>();

            foreach (var observer in observers)
            {
                try
                {
                    await observer.HandleEditTask(ctx);
                }
                catch (Exception e)
                {
                    this.logger?.LogError($"{observer.GetType().Name} throws an exception when handling editing:{Environment.NewLine}" +
                                          $"{e.Message}{Environment.NewLine}" +
                                          $"{e.StackTrace}");
                    
                    ctx.Cancel = true;
                }
                if (ctx.Cancel)
                {
                    ctx.Errors.Add(ErrorFactory.ObserverCancelExecution(observer));
                    return;
                }
            }
        }
        
        public async Task ExecuteTransition(
            ITaskObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITaskObserver>();

            foreach (var observer in observers)
            { 
                try
                {
                    await observer.HandleTransition(ctx);
                }
                catch (Exception e)
                {
                    this.logger?.LogError($"{observer.GetType().Name} throws an exception when handling transition:{Environment.NewLine}" +
                                          $"{e.Message}{Environment.NewLine}" +
                                          $"{e.StackTrace}");
                    
                    ctx.Cancel = true;
                    return;
                }
                
                if (ctx.Cancel)
                {
                    ctx.Errors.Add(ErrorFactory.ObserverCancelExecution(observer));
                    return;
                }
            }
        }
        
        #endregion
    }
}