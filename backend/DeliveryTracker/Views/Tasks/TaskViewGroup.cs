using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views.Tasks
{
    public class TaskViewGroup : ViewGroupBase
    {
        #region constuctor
        
        public TaskViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            ITaskService taskService) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();

            var managerOrder = 0;
            this.AddView(new ActualTasksManagerView(managerOrder++, taskService), dict);
            this.AddView(new MyTasksManagerView(managerOrder++, taskService), dict);
            this.AddView(new PreparingTasksManagerView(managerOrder++, taskService), dict);
            this.AddView(new QueueTasksManagerView(managerOrder++, taskService), dict);
            this.AddView(new CompleteTasksManager(managerOrder++, taskService), dict);
            this.AddView(new RevokedTasksManagerView(managerOrder, taskService), dict);

            var performerOrder = 100;
            this.AddView(new ActualTasksPerformerView(performerOrder++, taskService), dict);
            this.AddView(new QueueTasksPerformerView(performerOrder++, taskService), dict);
            this.AddView(new DeliveredTasksPerformerView(performerOrder++, taskService), dict);
            this.AddView(new CompletedTasksPerformerView(performerOrder++, taskService), dict);
            this.AddView(new RevokedTasksPerformerView(performerOrder, taskService), dict);
            
            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }
        
        #endregion

        #region base overrides

        public override string Name { get; } = nameof(TaskViewGroup);

        #endregion
        
    }
}