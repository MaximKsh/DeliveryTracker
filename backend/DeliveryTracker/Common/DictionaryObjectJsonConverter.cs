using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeliveryTracker.Common
{
    public sealed class DictionaryObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionaryObject);
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dict = serializer.Deserialize<IDictionary<string, object>>(reader);
            var dictObject = new DictionaryObject();
            dictObject.SetDictionary(dict);
            return dictObject;
        }
    }
}