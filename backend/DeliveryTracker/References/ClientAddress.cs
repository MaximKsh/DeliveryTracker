
using DeliveryTracker.Geopositioning;

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
        
        /// <summary>
        /// Координаты клиента
        /// </summary>
        public Geoposition Geoposition
        {
            get => this.Get<Geoposition>(nameof(this.Geoposition));
            set => this.Set(nameof(this.Geoposition), value);
        }
        
    }
}