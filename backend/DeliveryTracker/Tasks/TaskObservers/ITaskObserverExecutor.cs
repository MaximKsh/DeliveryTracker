using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TaskObservers
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