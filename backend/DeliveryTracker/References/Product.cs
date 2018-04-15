namespace DeliveryTracker.References
{
    public sealed class Product : ReferenceEntryBase
    {
        /// <summary>
        /// Артикул.
        /// </summary>
        public string VendorCode 
        {
            get => this.Get<string>(nameof(this.VendorCode));
            set => this.Set(nameof(this.VendorCode), value);
        }
        
        /// <summary>
        /// Название.
        /// </summary>
        public string Name 
        {
            get => this.Get<string>(nameof(this.Name));
            set => this.Set(nameof(this.Name), value);
        }
        
        /// <summary>
        /// Описание.
        /// </summary>
        public string Description
        {
            get => this.Get<string>(nameof(this.Description));
            set => this.Set(nameof(this.Description), value);
        }
        
        /// <summary>
        /// Стоимость.
        /// </summary>
        public decimal Cost 
        {
            get => this.Get<decimal>(nameof(this.Cost));
            set => this.Set(nameof(this.Cost), value);
        }
    }
}