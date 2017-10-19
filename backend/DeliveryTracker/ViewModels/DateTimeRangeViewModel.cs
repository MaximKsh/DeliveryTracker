using System;

namespace DeliveryTracker.ViewModels
{
    public class DateTimeRangeViewModel
    {
        /// <summary>
        /// Нижняя граница.
        /// </summary>
        public DateTime? From { get; set; }
        
        /// <summary>
        /// Верхняя граница.
        /// </summary>
        public DateTime? To { get; set; }
    }
}