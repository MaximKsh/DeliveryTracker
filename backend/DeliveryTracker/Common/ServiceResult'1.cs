using System.Collections.Generic;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Common
{
    /// <summary>
    /// Результат выполнения операции данными на выходе.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        public ServiceResult(params IError[] errors) : base(errors)
        {
        }
        
        public ServiceResult(IEnumerable<IError> errors = null) : base (errors)
        {
        }
        
        public ServiceResult(T result, params IError[] errors): base(errors)
        {
            this.Result = result;
        }
        
        public ServiceResult(T result, IEnumerable<IError> errors = null) : base(errors)
        {
            this.Result = result;
        }

        /// <summary>
        /// Результат выполнения операции.
        /// </summary>
        public T Result { get; } = default;

    }
}