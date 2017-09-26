using System;

namespace DeliveryTracker.ViewModels
{
    public class UpdateTaskStateViewModel
    {
        /// <summary>
        /// ID задания.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Новое состояние задания.
        /// </summary>
        public string State { get; set; }
    }
}