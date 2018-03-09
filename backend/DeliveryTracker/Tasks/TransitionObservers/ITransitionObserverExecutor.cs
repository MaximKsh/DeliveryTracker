using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public interface ITransitionObserverExecutor
    {
        Task Execute(
            ITransitionObserverContext ctx);
    }
}