using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class TaskObserverExecutor : ITaskObserverExecutor
    {
        #region fields

        private readonly IServiceProvider provider;

        //private readonly Logger<TaskObserverExecutor> logger;
        
        #endregion
        
        #region constuctor

        public TaskObserverExecutor(
            IServiceProvider provider)
        {
            this.provider = provider;
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
                    //this.logger.LogError(e, e.Message);
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
                    //this.logger.LogError(e, e.Message);
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
                    //this.logger.LogError(e, e.Message);
                }
            }
        }
        
        #endregion
    }
}