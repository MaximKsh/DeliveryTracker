using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public sealed class Geoposition : DictionarySerializableBase
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
        public override IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Longitude)] = this.Longitude,
                [nameof(this.Latitude)] = this.Latitude,
            };
        }

        /// <inheritdoc />
        public override void Deserialize(IDictionary<string, object> dict)
        {
            this.Longitude = GetPlain<double>(dict, nameof(this.Longitude));
            this.Latitude = GetPlain<double>(dict, nameof(this.Latitude));
        }
    }
}