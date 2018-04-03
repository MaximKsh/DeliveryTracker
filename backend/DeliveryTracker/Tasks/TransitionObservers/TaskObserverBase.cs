using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public abstract class TaskObserverBase : ITaskObserver
    {
        public virtual async Task HandleNewTask(
            ITaskObserverContext ctx)
        {
            await Task.CompletedTask;
        }

        public virtual async Task HandleEditTask(
            ITaskObserverContext ctx)
        {
            await Task.CompletedTask;
        }

        public virtual async Task HandleTransition(
            ITaskObserverContext ctx)
        {
            await Task.CompletedTask;
        }
    }
}