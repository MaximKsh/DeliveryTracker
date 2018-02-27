namespace DeliveryTracker.Views
{
    public class ViewDigest
    {
        /// <summary>
        /// Локализованный заголовок представления.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Количество элементов в представлении
        /// </summary>
        public long Count { get; set; }
        
        /// <summary>
        /// Тип сущности, к которому принадлежат элементы из резульата выборки
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// ПОрядок сортировки в списке представлений в группе.
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Название иконки у представления.
        /// </summary>
        public string IconName { get; set; }
    }
}