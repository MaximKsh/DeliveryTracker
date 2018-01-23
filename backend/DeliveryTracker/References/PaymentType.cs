namespace DeliveryTracker.References
{
    public class PaymentType : ReferenceEntityBase
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