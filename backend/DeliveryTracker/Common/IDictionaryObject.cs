using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public interface IDictionaryObject
    {
        void SetDictionary(
            IDictionary<string, object> dict);

        IDictionary<string, object> GetDictionary();

        T Cast<T>() where T : IDictionaryObject, new();
    }
}