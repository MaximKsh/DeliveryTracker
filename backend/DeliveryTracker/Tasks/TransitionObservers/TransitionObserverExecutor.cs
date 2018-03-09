using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class TransitionObserverExecutor : ITransitionObserverExecutor
    {
        #region fields

        private readonly IServiceProvider provider;
        
        #endregion
        
        #region constuctor

        public TransitionObserverExecutor(
            IServiceProvider provider)
        {
            this.provider = provider;
        }
        
        #endregion
        
        #region implementation
        
        public async Task Execute(
            ITransitionObserverContext ctx)
        {
            var observers = this.provider.GetServices<ITransitionObserver>();

            foreach (var observer in observers)
            {
                if (await observer.CanHandleTransition(ctx))
                {
                    await observer.HandleTransition(ctx);
                }
            }
        }
        
        #endregion
    }
}