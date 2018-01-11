using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeliveryTracker.Common
{
    public abstract class DictionarySerializableBase : IDictionarySerializable
    {
        /// <inheritdoc />
        public abstract IDictionary<string, object> Serialize();

        /// <inheritdoc />
        public abstract void Deserialize(IDictionary<string, object> dict);

        public static T GetPlain<T>(
            IDictionary<string, object> dict,
            string key, 
            T defaultValue = default)
        {
            if (dict.TryGetValue(key, out var val)
                && val is T result)
            {
                return result;
            }
            return defaultValue;
        }

        public static T GetObject<T>(
            T obj,
            IDictionary<string, object> dict,
            string key,
            T defaultValue = default)
            where T : IDictionarySerializable, new()
        {
            if (dict.TryGetValue(key, out var val)
                && val is IDictionary<string, object> result)
            {
                if (obj == null)
                {
                    obj = new T();
                }
                obj.Deserialize(result);
                return obj;
            }
            return defaultValue;
        }
    }
}