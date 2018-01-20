namespace DeliveryTracker.References
{
    public class Product : ReferenceEntityBase
    {
        /// <summary>
        /// Артикул.
        /// </summary>
        public string VendorCode { get; set; }
        
        /// <summary>
        /// Название.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Стоимость.
        /// </summary>
        public decimal Cost { get; set; }
    }
}