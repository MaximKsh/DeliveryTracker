
namespace DeliveryTracker.References
{
    public class Address : ReferenceCollectionBase
    {
        /// <summary>
        /// Неформатированный адрес.
        /// </summary>
        public string RawAddress 
        {
            get => this.Get<string>(nameof(this.RawAddress));
            set => this.Set(nameof(this.RawAddress), value);
        }
        
    }
}