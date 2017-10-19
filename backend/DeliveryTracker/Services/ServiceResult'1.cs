using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Services
{
    public class ServiceResult<T>
        where T: class
    {
        public ServiceResult(params IError[] errors)
        {
            this.Result = null;
            this.Errors = new ReadOnlyCollection<IError>(errors);
        }
        
        public ServiceResult(IEnumerable<IError> errors = null)
        {
            this.Result = null;
            this.Errors = new ReadOnlyCollection<IError>(
                errors?.ToList() ?? new List<IError>());
        }
        
        public ServiceResult(T result, params IError[] errors)
        {
            this.Result = result;
            this.Errors = new ReadOnlyCollection<IError>(errors);
        }
        
        public ServiceResult(T result, IEnumerable<IError> errors = null)
        {
            this.Result = result;
            this.Errors = new ReadOnlyCollection<IError>(
                errors?.ToList() ?? new List<IError>());
        }

        public T Result;

        public bool Success => this.Errors.Count == 0;
        
        public IReadOnlyList<IError> Errors { get; }

    }
}