using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Validation;

namespace DeliveryTrackerWeb.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public abstract class ResponseBase
    {
        protected ResponseBase()
        {
            
        }
        
        protected ResponseBase(IError error)
        {
            this.Errors = new List<IError> { error }.AsReadOnly();
        }
        
        protected ResponseBase(IReadOnlyList<IError> errors)
        {
            this.Errors = errors;
        }
        
        public IReadOnlyList<IError> Errors { get; set; }
        
    }
}