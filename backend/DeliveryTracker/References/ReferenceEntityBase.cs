using System;

namespace DeliveryTracker.References
{
    public abstract class ReferenceEntityBase
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Id инстанса.
        /// </summary>
        public Guid InstanceId { get; set; }
        
    }
}