using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskPackage : DictionaryObject
    {
        public IList<TaskInfo> TaskInfo
        {
            get => this.GetList<TaskInfo>(nameof(this.TaskInfo));
            set => this.Set(nameof(this.TaskInfo), value);
        }
        
        public IDictionary<string, IDictionaryObject> LinkedReferences
        {
            get => this.GetDictionaryField(nameof(this.LinkedReferences));
            set => this.Set(nameof(this.LinkedReferences), value);
        }

        public IList<User> LinkedUsers
        {
            get => this.GetList<User>(nameof(this.LinkedUsers));
            set => this.Set(nameof(this.LinkedUsers), value);
        }

        public IList<TaskStateTransition> LinkedTaskStateTransitions
        {
            get => this.GetList<TaskStateTransition>(nameof(this.LinkedTaskStateTransitions));
            set => this.Set(nameof(this.LinkedTaskStateTransitions), value);
        }
    }
}