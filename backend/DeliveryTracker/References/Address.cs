using System;

namespace DeliveryTracker.References
{
    public class Address : ReferenceEntityBase
    {
        /// <summary>
        /// Id клиента.
        /// </summary>
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// Неформатированный адрес.
        /// </summary>
        public string RawAddress { get; set; }
        
    }
}