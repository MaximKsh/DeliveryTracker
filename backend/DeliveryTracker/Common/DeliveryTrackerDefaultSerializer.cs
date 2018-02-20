
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeliveryTracker.Common
{
    public class DeliveryTrackerDefaultSerializer : IDeliveryTrackerSerializer
    {
        #region implementation
        
        /// <inheritdoc />
        public string SerializeJson(
            object obj) => JsonConvert.SerializeObject(obj);

        /// <inheritdoc />
        public string SerializeJson(
            IDictionaryObject obj) => JsonConvert.SerializeObject(obj.GetDictionary());

        /// <inheritdoc />
        public T DeserializeJson<T>(
            string serialized) => JsonConvert.DeserializeObject<T>(serialized);

        /// <inheritdoc />
        public T DeserializeJsonDictionaryObject<T>(
            string serialized) where T : IDictionaryObject, new()
        {
            var dict = JsonConvert.DeserializeObject<IDictionary<string, object>>(serialized);
            var obj = new T();
            obj.SetDictionary(dict);
            return obj;
        }
        
        #endregion
    }
}