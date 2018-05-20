using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeliveryTracker.Tasks.Routing.GoogleApiModels
{
    public sealed class GoogleDistanceMatrixResponse
    {
        [JsonProperty("destination_addresses")]
        public List<string> DestinationAddresses;
        
        [JsonProperty("origin_addresses")]
        public List<string> OriginAddresses;

        public List<GoogleRow> Rows;

        public string Status;

    }
}