using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Common
{
    public class ServiceResult
    {
        public ServiceResult(params IError[] errors)
        {
            this.Errors = new ReadOnlyCollection<IError>(errors);
        }
        
        public ServiceResult(IEnumerable<IError> errors = null)
        {
            this.Errors = new ReadOnlyCollection<IError>(
                errors?.ToList() ?? new List<IError>());
        }

        public bool Success => this.Errors.Count == 0;
        
        public IReadOnlyList<IError> Errors { get; }

    }
}