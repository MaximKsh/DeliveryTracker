using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public interface IDictionarySerializable
    {
        IDictionary<string, object> Serialize();
        void Deserialize(IDictionary<string, object> dict);
    }
}