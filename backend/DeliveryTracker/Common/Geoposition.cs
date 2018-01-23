namespace DeliveryTracker.Common
{
    public sealed class Geoposition : DictionaryObject
    {
        /// <summary>
        /// Долгота.
        /// </summary>
        public double Longitude
        {
            get => this.Get<double>(nameof(this.Longitude));
            set => this.Set(nameof(this.Longitude), value);
        }
        
        
        /// <summary>
        /// Широта.
        /// </summary>
        public double Latitude
        {
            get => this.Get<double>(nameof(this.Latitude));
            set => this.Set(nameof(this.Latitude), value);
        }
        
    }
}