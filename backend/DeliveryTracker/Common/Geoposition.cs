using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public sealed class Geoposition : IDictionarySerializable
    {
        /// <summary>
        /// Долгота.
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// Широта.
        /// </summary>
        public double Latitude { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Longitude)] = this.Longitude,
                [nameof(this.Latitude)] = this.Latitude,
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Longitude = dict.GetPlain<double>(nameof(this.Longitude));
            this.Latitude = dict.GetPlain<double>(nameof(this.Latitude));
        }
    }
}