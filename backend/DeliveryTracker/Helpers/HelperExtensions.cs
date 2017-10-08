using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DeliveryTracker.Helpers
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
        
    }
}