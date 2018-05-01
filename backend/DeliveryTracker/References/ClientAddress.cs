
using DeliveryTracker.Geopositioning;

namespace DeliveryTracker.References
{
    public sealed class ClientAddress : ReferenceCollectionBase
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
            get => this.GetObject<Geoposition>(nameof(this.Geoposition));
            set => this.Set(nameof(this.Geoposition), value);
        }
        
    }
}