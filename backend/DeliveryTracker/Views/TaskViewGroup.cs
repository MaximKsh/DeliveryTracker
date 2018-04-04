using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public class TaskViewGroup : ViewGroupBase
    {
        #region constuctor
        
        public TaskViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();

            var managerOrder = 0;
            this.AddView(new ActualTasksManagerView(managerOrder++), dict);
            this.AddView(new MyTasksManagerView(managerOrder++), dict);
            this.AddView(new PreparingTasksManagerView(managerOrder++), dict);
            this.AddView(new QueueTasksManagerView(managerOrder++), dict);
            this.AddView(new CompleteTasksManager(managerOrder++), dict);
            this.AddView(new RevokedTasksManagerView(managerOrder), dict);

            var performerOrder = 100;
            this.AddView(new ActualTasksPerformerView(performerOrder++), dict);
            this.AddView(new QueueTasksPerformerView(performerOrder++), dict);
            this.AddView(new DeliveredTasksPerformerView(performerOrder++), dict);
            this.AddView(new CompletedTasksPerformerView(performerOrder++), dict);
            this.AddView(new RevokedTasksPerformerView(performerOrder), dict);
            
            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }
        
        #endregion

        #region base overrides

        public override string Name { get; } = nameof(TaskViewGroup);

        #endregion
        
    }
}