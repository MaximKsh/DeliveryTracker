using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryTracker.Common
{
    public class DeliveryTrackerSerializer : IDeliveryTrackerSerializer
    {
        #region fields
        
        private readonly JsonSerializerSettings settings;
        
        #endregion
        
        #region constructor
        
        public DeliveryTrackerSerializer(
            IOptions<MvcJsonOptions> mvcJsonOptions)
        {
            this.settings = mvcJsonOptions.Value.SerializerSettings;
        }
        
        #endregion
        
        #region implementation
        
        /// <inheritdoc />
        public string SerializeJson(
            object obj) => JsonConvert.SerializeObject(obj, this.settings);

        /// <inheritdoc />
        public string SerializeJson(
            IDictionaryObject obj) => JsonConvert.SerializeObject(obj.GetDictionary(), this.settings);

        /// <inheritdoc />
        public T DeserializeJson<T>(
            string serialized) => JsonConvert.DeserializeObject<T>(serialized, this.settings);

        /// <inheritdoc />
        public T DeserializeJsonDictionaryObject<T>(
            string serialized) where T : IDictionaryObject, new()
        {
            var dict = JsonConvert.DeserializeObject<IDictionary<string, object>>(serialized, this.settings);
            var obj = new T();
            obj.SetDictionary(dict);
            return obj;
        }
        
        #endregion
    }
}