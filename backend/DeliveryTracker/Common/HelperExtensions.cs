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
        
        
        #region IDictionaryExtensions

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, 
            TKey key, 
            TValue defaultValue = default)
        {
            return dict.TryGetValue(key, out var val) 
                ? val 
                : defaultValue;
        }

        public static T GetTypifiedValueOrDefault<T>(
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
        
        #endregion 
       
    }
}