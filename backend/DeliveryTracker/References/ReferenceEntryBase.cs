using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public class ReferenceEntryBase : DictionaryObject
    {
        public ReferenceEntryBase()
        {
            this.Type = this.GetType().Name;
        }
        
        /// <summary>
        /// Тип элемента.
        /// Менять самостоятельно не рекомендуется.
        /// </summary>
        public string Type 
        {
            get => this.Get<string>(nameof(this.Type));
            set => this.Set(nameof(this.Type), value);
        }
        
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }

        /// <summary>
        /// Id инстанса.
        /// </summary>
        public Guid InstanceId
        {
            get => this.Get<Guid>(nameof(this.InstanceId));
            set => this.Set(nameof(this.InstanceId), value);
        }

        /// <summary>
        /// Сущность удалена.
        /// </summary>
        public bool Deleted
        {
            get => this.Get<bool>(nameof(this.Deleted));
            set => this.Set(nameof(this.Deleted), value);
        }
    }
}