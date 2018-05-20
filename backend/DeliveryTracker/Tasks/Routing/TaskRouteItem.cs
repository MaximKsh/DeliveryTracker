using System;
using Newtonsoft.Json;

namespace DeliveryTracker.Tasks.Routing
{
    public sealed class TaskRouteItem
    {
        /// <summary>
        /// Идентификатор задания.
        /// </summary>
        /// 
        [JsonProperty("taskId")]
        public Guid TaskId;

        /// <summary>
        /// Исполнитель задания.
        /// </summary>
        [JsonProperty("performerId")]
        public Guid? PerformerId;

        /// <summary>
        /// Время открытия окна в секундах от начала суток.
        /// </summary>
        [JsonProperty("startTimeOffset")]
        public int StartTimeOffset;

        /// <summary>
        /// Время закрытия окна в секундах от начала суток
        /// </summary>
        [JsonProperty("endTimeOffset")]
        public int EndTimeOffset;
        
        /// <summary>
        /// Долгота.
        /// </summary>
        [JsonIgnore]
        public double Longitude { get; set; }
        
        /// <summary>
        /// Широта.
        /// </summary>
        [JsonIgnore]
        public double Latitude { get; set; }
    }
}