namespace DeliveryTracker.ViewModels
{
    public class AvailablePerformerViewModel
    {
        /// <summary>
        /// Имя(уникальное) исполнителя.
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Отображаемое имя исполнителя.
        /// </summary>
        public string DisplayableName { get; set; }
        
        /// <summary>
        /// Текущая позиция исполнителя.
        /// </summary>
        public GeopositionViewModel Position { get; set; }
    }
}