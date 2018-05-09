using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public interface ITaskObserver
    {
        Task HandleNewTask(
            ITaskObserverContext ctx);

        Task HandleEditTask(
            ITaskObserverContext ctx);
        
        Task HandleTransition(
            ITaskObserverContext ctx);
        
    }
}