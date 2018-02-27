using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DeliveryTracker.Common
{
    public abstract class DictionaryObject : IDictionaryObject
    {
        #region fields
        
        private IDictionary<string, object> dictionary = 
            new Dictionary<string, object>();
        
        #endregion
        
        #region public
        
        public void SetDictionary(
            IDictionary<string, object> dict)
        {
            this.dictionary = dict ?? throw new ArgumentNullException();
        }

        public IDictionary<string, object> GetDictionary()
        {
            this.BeforeGetDictionary();
            foreach (var k in this.dictionary.Keys.ToArray())
            {
                var v = this.dictionary[k];
                if(v is IDictionaryObject dictObj)
                {
                    this.dictionary[k] = dictObj.GetDictionary();
                }

                if (v is IList list)
                {
                    var newList = new List<object>(list.Count);
                    foreach (var elem in list)
                    {
                        if(elem is IDictionaryObject listElemDictObj)
                        {
                            newList.Add(listElemDictObj.GetDictionary());
                        }
                        else
                        {
                            newList.Add(elem);
                        }
                    }

                    this.dictionary[k] = newList;
                }
            }
            
            this.AfterGetDictionary();
            return this.dictionary;
        }
        
        #endregion
        
        #region protected

        protected virtual void BeforeGetDictionary()
        {
            
        }

        protected virtual void AfterGetDictionary()
        {
            
        }
        
        protected T Get<T>(
            string key,
            T defaultValue = default)
        {
            if (!this.dictionary.TryGetValue(key, out var obj))
            {
                return defaultValue;
            }

            if (obj == null)
            {
                return defaultValue;
            }
            
            if (obj is T value)
            {
                return value;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                var result = (T) converter.ConvertFrom(obj);
                this.dictionary[key] = result;
                return result;
            }
            catch (Exception)
            {
                return defaultValue;                
            }
        }
        
        protected T GetObject<T>(
            string key,
            T defaultValue = default)  where T : IDictionaryObject, new ()
        {
            if (!this.dictionary.TryGetValue(key, out var obj))
            {
                return defaultValue;
            }

            switch (obj)
            {
                case null:
                    return defaultValue;
                case T value:
                    return value;
                case JObject jobj:
                    var result = jobj.ToObject<T>();
                    this.dictionary[key] = result;
                    return result;
                case IDictionary<string, object> dict:
                    var deserialized = new T();
                    deserialized.SetDictionary(dict);
                    this.dictionary[key] = deserialized;
                    return deserialized;
            }
            return defaultValue;
        }
        
        protected IList<T> GetList<T>(
            string key,
            IList<T> defaultValue = default)  where T : IDictionaryObject, new ()
        {
            if (!this.dictionary.TryGetValue(key, out var obj))
            {
                return defaultValue;
            }

            switch (obj)
            {
                case IList<T> value:
                    return value;
                case IList<JToken> jsonObjectList:
                {
                    var result = new List<T>(jsonObjectList.Count);
                    foreach (var item in jsonObjectList)
                    {
                        result.Add(item.ToObject<T>());
                    }
                    
                    this.dictionary[key] = result;
                    return result;
                }
                case IList<IDictionary<string, object>> listDict:
                {
                    var result = new List<T>(listDict.Count);
                    foreach (var item in listDict)
                    {
                        var deserialized = new T();
                        deserialized.SetDictionary(item);
                        result.Add(deserialized);
                    }

                    this.dictionary[key] = result;
                    return result;
                }
            }

            return defaultValue;
        }
        
        protected void Set(
            string key,
            object value)
        {
            this.dictionary[key] = value;
        }
        
        #endregion
    }
}