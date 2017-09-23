using System.Linq;
using backend.ViewModels.Errors;
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
                            new ErrorItemViewModel {Code = p.Key, Message = q.ErrorMessage}))
                    .ToArray()
            };
        }
        
        
        #endregion
        
        
    }
}