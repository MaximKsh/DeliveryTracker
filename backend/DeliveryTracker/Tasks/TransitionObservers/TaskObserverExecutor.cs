using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class TaskObserverExecutor : ITaskObserverExecutor
    {
        #region fields

        private readonly IServiceProvider provider;
        
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
                await observer.HandleNewTask(ctx);
            }
        }
        
        public async Task ExecuteEdit(
            ITaskObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITaskObserver>();

            foreach (var observer in observers)
            { 
                await observer.HandleEditTask(ctx);
            }
        }
        
        public async Task ExecuteTransition(
            ITaskObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITaskObserver>();

            foreach (var observer in observers)
            { 
                await observer.HandleTransition(ctx);
            }
        }
        
        #endregion
    }
}