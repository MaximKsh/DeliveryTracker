using System;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Notifications;
using Npgsql;

namespace DeliveryTracker.Tasks.TaskObservers
{
    public sealed class NotificationObserver : TaskObserverBase
    {
        private readonly INotificationService notificationService;

        private readonly IDeviceManager deviceManager;

        public NotificationObserver(
            INotificationService notificationService,
            IDeviceManager deviceManager)
        {
            this.notificationService = notificationService;
            this.deviceManager = deviceManager;
        }

        public override async Task HandleEditTask(
            ITaskObserverContext ctx)
        {
            if (ctx.TaskChanges.PerformerId.HasValue)
            {
                Guid? prevPerformer;
                using (var command = ctx.ConnectionWrapper.CreateCommand())
                {
                    command.CommandText = "select performer_id from tasks where id = @id and instance_id = @instance_id;";
                    command.Parameters.Add(new NpgsqlParameter("id", ctx.TaskInfo.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", ctx.Credentials.InstanceId));
                    prevPerformer = (Guid?)await command.ExecuteScalarAsync();
                }

                if (prevPerformer != ctx.TaskChanges.PerformerId.Value)
                {
                    await this.SendPush(ctx, ctx.TaskInfo.PerformerId, LocalizationAlias.PushMessage.TaskPlannedToYou);
                }
                
            }
        }
        
        public override async Task HandleTransition(
            ITaskObserverContext ctx)
        {
            if (ctx.Transition.FinalState == DefaultTaskStates.Queue.Id)
            {
                await this.SendPush(ctx, ctx.TaskInfo.PerformerId, LocalizationAlias.PushMessage.TaskPlannedToYou);
            }
            
            if (ctx.Transition.FinalState == DefaultTaskStates.Waiting.Id)
            {
                await this.SendPush(ctx, ctx.TaskInfo.PerformerId, LocalizationAlias.PushMessage.TaskWaits);
            }
            
            if (ctx.Transition.FinalState == DefaultTaskStates.Revoked.Id)
            {
                await this.SendPush(ctx, ctx.TaskInfo.PerformerId, LocalizationAlias.PushMessage.TaskRevoked);
            }
            
            if (ctx.Transition.FinalState == DefaultTaskStates.Preparing.Id)
            {
                await this.SendPush(ctx, ctx.TaskInfo.PerformerId, LocalizationAlias.PushMessage.TaskReturnToPreparing);
            }
            
            if (ctx.Transition.FinalState == DefaultTaskStates.Delivered.Id)
            {
                await this.SendPush(ctx, ctx.TaskInfo.AuthorId, LocalizationAlias.PushMessage.TaskDelivered);
            }
        }

        private async Task SendPush(
            ITaskObserverContext ctx,
            Guid? userId, 
            string text)
        {
            if (!userId.HasValue)
            {
                return;
            }
            
            
            var deviceResult = await this.deviceManager.GetUserDeviceAsync(userId.Value, ctx.ConnectionWrapper);
            if (!deviceResult.Success)
            {
                return;
            }

            var device = deviceResult.Result;
            var notification = new Notification();
            notification.Components.Add(new PushNotificationComponent
            {
                Device = device,
                Body = new PushNotificationBody
                {
                    Action = PushActions.OpenTask,
                    Title = LocalizationAlias.PushMessage.TaskHeader,
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