using DeliveryTracker.Common;

namespace DeliveryTracker.Tasks.Routing
{
    public class RoutingSettings : ISettings
    {
        public RoutingSettings(
            string name,
            string routingServiceUrl,
            string distanceMatrixApiKey)
        {
            this.Name = name;
            this.RoutingServiceUrl = routingServiceUrl;
            this.DistanceMatrixApiKey = distanceMatrixApiKey;
        }

        /// <inheritdoc />
        public string Name { get; }
        
        public string RoutingServiceUrl { get; }
        
        public string DistanceMatrixApiKey { get; }
    }
}