using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public interface ITransitionObserver
    {
        Task<bool> CanHandleTransition(
            ITransitionObserverContext ctx);

        Task HandleTransition(
            ITransitionObserverContext ctx);

    }
}