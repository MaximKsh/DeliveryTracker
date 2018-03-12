using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DeliveryTracker.Common
{
    public class DictionaryObject : IDictionaryObject
    {
        #region fields
        
        protected IDictionary<string, object> Dictionary = 
            new Dictionary<string, object>();
        
        #endregion

        #region public
        
        public virtual void SetDictionary(
            IDictionary<string, object> dict)
        {
            this.Dictionary = dict ?? throw new ArgumentNullException();
        }

        public virtual IDictionary<string, object> GetDictionary()
        {
            foreach (var k in this.Dictionary.Keys.ToArray())
            {
                var v = this.Dictionary[k];
                if (v is IDictionaryObject dictObj)
                {
                    this.Dictionary[k] = dictObj.GetDictionary();
                }
                else if (v is IDictionary<string, IDictionaryObject> dictField)
                {
                    var newDict = new Dictionary<string, IDictionary<string, object>>((int)(1.5 * dictField.Count));
                    foreach (var pair in dictField)
                    {
                        newDict[pair.Key] = pair.Value.GetDictionary();
                    }

                    this.Dictionary[k] = newDict;
                }
                else if (v is IList list)
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

                    this.Dictionary[k] = newList;
                }
            }
            
            return this.Dictionary;
        }

        public T Cast<T>() where T : IDictionaryObject, new()
        {
            var newObj = new T();
            newObj.SetDictionary(this.Dictionary);
            return newObj;
        }

        #endregion
        
        #region protected

        protected virtual T Get<T>(
            string key,
            T defaultValue = default)
        {
            if (!this.Dictionary.TryGetValue(key, out var obj))
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
                T result;
                if (converter.CanConvertFrom(obj.GetType()))
                {
                    result = (T) converter.ConvertFrom(obj);
                }
                else
                {
                    result = (T) converter.ConvertFromString(obj.ToString());
                }
                
                this.Dictionary[key] = result;
                return result;
            }
            catch (Exception)
            {
                return defaultValue;                
            }
        }
        
        protected virtual T GetObject<T>(
            string key,
            T defaultValue = default)  where T : IDictionaryObject, new ()
        {
            if (!this.Dictionary.TryGetValue(key, out var obj))
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
                    this.Dictionary[key] = result;
                    return result;
                case IDictionary<string, object> dict:
                    var deserialized = new T();
                    deserialized.SetDictionary(dict);
                    this.Dictionary[key] = deserialized;
                    return deserialized;
            }
            return defaultValue;
        }

        protected virtual IDictionary<string, T> GetDictionaryField <T> (
            string key,
            IDictionary<string, T> defaultValue = default)
            where T : IDictionaryObject, new ()
        {
            if (!this.Dictionary.TryGetValue(key, out var obj))
            {
                return defaultValue;
            }

            switch (obj)
            {
                case null:
                    return defaultValue;
                case IDictionary<string, T> dict:
                    return dict;
                case IDictionary<string, object> dictStringObject:
                {
                    var result = new Dictionary<string, T>((int)(1.5 * dictStringObject.Count));
                    foreach (var pair in dictStringObject)
                    {
                        switch (pair.Value)
                        {
                            case T itemCorrect:
                                result.Add(pair.Key, itemCorrect);
                                break;
                            case IDictionary<string, object> itemDict:
                                var entity = new T();
                                entity.SetDictionary(itemDict);
                                result.Add(pair.Key, entity);
                                break;
                            default:
                                result.Add(pair.Key, default);
                                break;
                        }    
                    }
                    this.Dictionary[key] = result;
                    return result;
                }
                case JObject jobj:
                {
                    var result = new Dictionary<string, T>((int)(1.5 * jobj.Count));
                    foreach (var pair in jobj)
                    {
                        var dictionary = pair.Value.ToObject<IDictionary<string, object>>();
                        var dictObj = new T();
                        dictObj.SetDictionary(dictionary);
                        result.Add(pair.Key, dictObj);
                    }
                    this.Dictionary[key] = result;
                    return result;
                }
            }
            return defaultValue;
        }
        
        protected virtual IList<T> GetList<T>(
            string key,
            IList<T> defaultValue = default)  where T : IDictionaryObject, new ()
        {
            if (!this.Dictionary.TryGetValue(key, out var obj))
            {
                return defaultValue;
            }

            switch (obj)
            {
                case null:
                    return null;
                case IList<T> value:
                    return value;
                case IList<object> valueObj:
                {
                    var result = new List<T>(valueObj.Count);
                    foreach (var item in valueObj)
                    {
                        switch (item)
                        {
                            case T itemCorrect:
                                result.Add(itemCorrect);
                                break;
                            case IDictionary<string, object> itemDict:
                                var entity = new T();
                                entity.SetDictionary(itemDict);
                                result.Add(entity);
                                break;
                            default:
                                result.Add(default);
                                break;
                        }
                    }
                    this.Dictionary[key] = result;
                    return result;   
                }
                case IList<JToken> jsonObjectList:
                {
                    var result = new List<T>(jsonObjectList.Count);
                    foreach (var item in jsonObjectList)
                    {
                        result.Add(item.ToObject<T>());
                    }
                    
                    this.Dictionary[key] = result;
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

                    this.Dictionary[key] = result;
                    return result;
                }
            }

            return defaultValue;
        }
        
        protected virtual void Set(
            string key,
            object value)
        {
            this.Dictionary[key] = value;
        }
        
        #endregion

    }
}