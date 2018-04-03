using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
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