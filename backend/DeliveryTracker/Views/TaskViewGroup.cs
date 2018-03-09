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
            
            var tasksManagerView = new TasksManagerView(0);
            dict[tasksManagerView.Name] = tasksManagerView;
            var tasksPerformersView = new TasksPerformerView(100);
            dict[tasksPerformersView.Name] = tasksPerformersView;
            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }
        
        #endregion

        #region base overrides

        public override string Name { get; } = nameof(TaskViewGroup);

        #endregion
    }
}