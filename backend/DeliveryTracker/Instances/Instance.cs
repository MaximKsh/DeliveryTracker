using System;

namespace DeliveryTracker.Instances
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Instance 
    {
        /// <summary>
        /// Id группы.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя группы.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Имя группы.
        /// </summary>
        public Guid CreatorId { get; set; }

    }
}