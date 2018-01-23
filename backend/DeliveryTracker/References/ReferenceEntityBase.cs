using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public abstract class ReferenceEntityBase : DictionaryObject
    {
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
        
    }
}