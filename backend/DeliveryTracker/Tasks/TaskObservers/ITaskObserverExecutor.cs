using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public interface ITaskObserverExecutor
    {
        Task ExecuteNew(
            ITaskObserverContext ctx);
        
        Task ExecuteEdit(
            ITaskObserverContext ctx);
        
        Task ExecuteTransition(
            ITaskObserverContext ctx);
    }
}