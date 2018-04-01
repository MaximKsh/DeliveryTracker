using System.Threading.Tasks;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Notifications;

namespace DeliveryTracker.Tasks.TransitionObservers
{
    public class CompleteObserver : TransitionObserverBase
    {
        private readonly INotificationService notificationService;

        private readonly IDeviceManager deviceManager;

        private readonly IPostgresConnectionProvider cp;
        
        public CompleteObserver(
            INotificationService notificationService,
            IDeviceManager deviceManager,
            IPostgresConnectionProvider cp)
        {
            this.notificationService = notificationService;
            this.deviceManager = deviceManager;
            this.cp = cp;
        }
        
        public override Task<bool> CanHandleTransition(
            ITransitionObserverContext ctx)
        {
            return Task.FromResult(false);
        }

        public override async Task HandleTransition(
            ITransitionObserverContext ctx)
        {
            var userId = ctx.TaskInfo.AuthorId;
            Device device;
            using (var conn = ctx.ConnectionWrapper.Connect() ?? this.cp.Create().Connect())
            {
                var deviceResult = await this.deviceManager.GetUserDeviceAsync(userId, conn);
                if (!deviceResult.Success)
                {
                    return;
                }

                device = deviceResult.Result;
            }
            var notification = new Notification();
            var text = ctx.Transition.FinalState == DefaultTaskStates.Complete.Id
                ? "Your task is completed"
                : "Your task is cancelled";
            
            notification.Components.Add(new PushNotificationComponent
            {
                Device = device,
                Body = new PushNotificationBody
                {
                    Action = PushActions.OpenTask,
                    Title = "Task completed",
                    Message = text,
                    Data = new TaskInfo
                    {
                        Id = ctx.TaskInfo.Id,
                    }
                }
                
            });
            
            this.notificationService.SendNotification(notification);
        }
        
    }
}