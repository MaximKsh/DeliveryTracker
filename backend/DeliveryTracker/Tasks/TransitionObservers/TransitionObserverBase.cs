using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public abstract class TransitionObserverBase : ITransitionObserver
    {
        public virtual async Task<bool> CanHandleTransition(
            ITransitionObserverContext ctx)
        {
            return true;
        }

        public virtual async Task HandleTransition(
            ITransitionObserverContext ctx)
        {
        }
    }
}