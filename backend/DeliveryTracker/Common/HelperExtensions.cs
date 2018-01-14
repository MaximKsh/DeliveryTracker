using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Common
{
    public static class HelperExtensions
    {
        #region ModelState extension

        public static ErrorListViewModel ToErrorListViewModel(this ModelStateDictionary modelState)
        {
            return new ErrorListViewModel
            {
                Errors = modelState
                    .SelectMany(p =>
                        p.Value.Errors.Select(q =>
                            new ErrorItemViewModel
                            {
                                Code = ErrorCode.InvalidInputParameter,
                                Message = q.ErrorMessage,
                            }))
                    .ToArray()
            };
        }
        
        
        #endregion

        #region IError extensions
        
        public static ErrorItemViewModel ToErrorItemViewModel(this IError error)
        {
            return new ErrorItemViewModel
            {
                Code = error.Code,
                Message = error.Message,
                Info = error.Info.ToDictionary(k => k.Key, v => v.Value),
            };
        }
        
        public static ErrorListViewModel ToErrorListViewModel(this IError error)
        {
            return new ErrorListViewModel
            {
                Errors = new List<ErrorItemViewModel>
                {
                    new ErrorItemViewModel
                    {
                        Code = error.Code,
                        Message = error.Message,
                        Info = error.Info.ToDictionary(k => k.Key, v => v.Value),
                    }
                }
            };
        }

        public static ErrorListViewModel ToErrorListViewModel(this IEnumerable<IError> errors)
        {
            return new ErrorListViewModel
            {
                Errors = errors.Select(p => p.ToErrorItemViewModel()).ToArray(),
            };
        }
        
        #endregion
        
        #region Logger extensions

        public static void Trace<T>(this ILogger<T> logger, string username, string message)
        {
            logger.LogTrace($"[{username}]: {message}");
        }
        
        #endregion
        
        
        #region IDictionarySerializableExtensions

        public static T GetPlain<T>(
            this IDictionary<string, object> dict,
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
            this IDictionary<string, object> dict,
            string key,
            T defaultValue = default)
            where T : IDictionarySerializable, new()
        {
            if (dict.TryGetValue(key, out var val)
                && val is IDictionary<string, object> result)
            {
                var obj = new T();
                obj.Deserialize(result);
                return obj;
            }
            return defaultValue;
        }

        public static IList<T> GetPlainList<T>(
            this IDictionary<string, object> dict,
            string key,
            IList<T> defaultValue = default,
            T defaultElementValue = default)
        {
            if (dict.TryGetValue(key, out var val)
                && val is IEnumerable<object> result)
            {
                var newList = new List<T>();
                foreach (var item in result)
                {
                    var newItem = item is T typifiedItem
                        ? typifiedItem
                        : defaultElementValue;
                    newList.Add(newItem);
                }
                return newList;
            }
            return defaultValue;
        }
        
        public static IList<T> GetObjectList<T>(
            this IDictionary<string, object> dict,
            string key,
            IList<T> defaultValue = default,
            Func<T> defaultElementValueFactory = null)
            where T : IDictionarySerializable, new()
        {
            if (dict.TryGetValue(key, out var val)
                && val is IEnumerable<object> result)
            {
                var newList = new List<T>();
                foreach (var item in result)
                {
                    if (item is Dictionary<string, object> itemDict)
                    {
                        var newItem = new T();
                        newItem.Deserialize(itemDict);
                        newList.Add(newItem);
                    }
                    else if (defaultElementValueFactory != null)
                    {
                        newList.Add(defaultElementValueFactory());
                    }
                }
                return newList;
            }
            return defaultValue;
        }

        public static object SerializePlainList(this IEnumerable<object> list)
        {
            return list;
        }

        public static object SerializeObjectList<T>(this IEnumerable<T> list) where T : IDictionarySerializable
        {
            var serialized = new List<IDictionary<string, object>>();
            foreach (var item in list)
            {
                serialized.Add(item.Serialize());
            }
            return serialized;
        }
        
        #endregion 
       
    }
}