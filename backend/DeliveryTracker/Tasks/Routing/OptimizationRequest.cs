using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeliveryTracker.Tasks.Routing
{
    public sealed class OptimizationRequest
    {
        [JsonProperty("tasks")]
        public List<TaskRouteItem> Tasks { get; set; }
        [JsonProperty("performers")]
        public List<Guid> Performers { get; set; }
        [JsonProperty("weights")]
        public List<List<int>> Weights { get; set; }
        [JsonProperty("tryKeepPerformers")]
        public bool TryKeepPerformers { get; set; }
    }
}