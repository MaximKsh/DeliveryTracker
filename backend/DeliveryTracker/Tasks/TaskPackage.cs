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
        
        public IDictionary<string, DictionaryObject> LinkedReferences
        {
            get => this.GetDictionaryField<DictionaryObject>(nameof(this.LinkedReferences));
            set => this.Set(nameof(this.LinkedReferences), value);
        }

        public IDictionary<string, User> LinkedUsers
        {
            get => this.GetDictionaryField<User>(nameof(this.LinkedUsers));
            set => this.Set(nameof(this.LinkedUsers), value);
        }

        public IList<TaskProduct> TaskProducts
        {
            get => this.GetList<TaskProduct>(nameof(this.TaskProducts));
            set => this.Set(nameof(this.TaskProducts), value);
        }
    }
}