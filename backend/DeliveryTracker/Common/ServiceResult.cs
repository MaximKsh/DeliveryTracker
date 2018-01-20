using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Common
{
    /// <summary>
    /// Результат выполнения операции.
    /// </summary>
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

        /// <summary>
        /// Операция произошла без ошибок.
        /// </summary>
        public bool Success => this.Errors.Count == 0;
        
        /// <summary>
        /// Список ошибок.
        /// </summary>
        public IReadOnlyList<IError> Errors { get; }

    }
}