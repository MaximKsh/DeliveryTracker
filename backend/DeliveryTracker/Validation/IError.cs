using System;
using System.Collections.Generic;

namespace DeliveryTracker.Validation
{
    public interface IError : IEquatable<IError>
    {
        /// <summary>
        /// Код ошибки.
        /// </summary>
        string Code { get; }
        
        /// <summary>
        /// Отображаемое сообщение об ошибке.
        /// </summary>
        string Message { get; }
        
        /// <summary>
        /// Дополнительная информация по ошибке.
        /// </summary>
        IReadOnlyDictionary<string, string> Info { get; }
    }
}