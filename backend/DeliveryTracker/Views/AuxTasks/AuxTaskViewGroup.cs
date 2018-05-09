using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views.AuxTasks
{
    public sealed class AuxTaskViewGroup : ViewGroupBase
    {
        public AuxTaskViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            ITaskService taskService) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();

            this.AddView(new UserTasksView(0, taskService), dict);
            
            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }

        public override string Name => nameof(AuxTaskViewGroup);
    }
}