using System;
using DeliveryTracker.Validation;
using Newtonsoft.Json;

namespace DeliveryTrackerWeb.Tests.Validation
{
    public sealed class ErrorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IError);
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(Error));
        }
    }
}