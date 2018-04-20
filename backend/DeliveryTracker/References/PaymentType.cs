namespace DeliveryTracker.References
{
    public sealed class PaymentType : ReferenceEntryBase
    {
        
        /// <summary>
        /// Название типа оплаты.
        /// </summary>
        public string Name 
        {
            get => this.Get<string>(nameof(this.Name));
            set => this.Set(nameof(this.Name), value);
        }
    }
}