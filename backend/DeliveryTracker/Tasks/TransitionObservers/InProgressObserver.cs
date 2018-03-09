using System;
using System.Threading.Tasks;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class InProgressObserver : TransitionObserverBase
    {
        
        #pragma warning disable 1998
        
        public override async Task<bool> CanHandleTransition(
            ITransitionObserverContext ctx)
        {
            return ctx.Transition.FinalState == DefaultTaskStates.InProgress.Id;
        }

        public override async Task HandleTransition(
            ITransitionObserverContext ctx)
        {
            ctx.TaskInfo.ReceiptActual = DateTime.UtcNow;
        }
    }
}