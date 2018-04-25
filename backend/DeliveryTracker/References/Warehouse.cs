using DeliveryTracker.Geopositioning;

namespace DeliveryTracker.References
{
    public sealed class Warehouse : ReferenceEntryBase
    {
        /// <summary>
        /// Название.
        /// </summary>
        public string Name 
        {
            get => this.Get<string>(nameof(this.Name));
            set => this.Set(nameof(this.Name), value);
        }
        
        /// <summary>
        /// Неформатированный адрес.
        /// </summary>
        public string RawAddress 
        {
            get => this.Get<string>(nameof(this.RawAddress));
            set => this.Set(nameof(this.RawAddress), value);
        }
        
        /// <summary>
        /// Координаты.
        /// </summary>
        public Geoposition Geoposition
        {
            get => this.GetObject<Geoposition>(nameof(this.Geoposition));
            set => this.Set(nameof(this.Geoposition), value);
        }
    }
}