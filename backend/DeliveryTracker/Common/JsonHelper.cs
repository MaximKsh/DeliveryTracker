using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DeliveryTracker.Common
{
    public static class JsonHelper
    {
        public static IDictionary<string, object> GetSubdictionary(
            IDictionary<string, object> root,
            string key)
        {
            TryGetSubdictionary(root, key, out var result);
            return result;
        }
        
        public static bool TryGetSubdictionary(
            IDictionary<string, object> root,
            string key, 
            out IDictionary<string, object> result)
        {
            result = null;
            if (!root.TryGetValue(key, out var valueObj))
            {
                return false;
            }
            
            switch (valueObj)
            {
                case IDictionary<string, object> value:
                    result = value;
                    return true;
                case JObject valueJObj:
                    result =  valueJObj.Children<JProperty>()
                        .ToDictionary(prop => prop.Name,
                            prop => ToObject(prop.Value));
                    return true;
                default:
                    return false;
            }
        }


        public static bool TryGetSubdictionaryList(
            IDictionary<string, object> root,
            string key, 
            out IList<IDictionary<string, object>> result)
        {
            result = null;
            if (!root.TryGetValue(key, out var valueObj))
            {
                return false;
            }
            
            switch (valueObj)
            {
                case IList<IDictionary<string, object>> value:
                    result = value;
                    return true;
                case JArray valueJArr:
                    result = valueJArr
                        .Select(t =>
                        {
                            return (IDictionary<string, object>) t.Children<JProperty>()
                                    .ToDictionary(prop => prop.Name, prop => ToObject(prop.Value));
                        })
                        .ToList();
                    return true;
                default:
                    return false;
            }
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                        .ToDictionary(prop => prop.Name,
                            prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();;
                default:
                    return ((JValue)token).Value;
            }
        }
    }
}