using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ErrorItemViewModel
    {
        /// <summary>
        /// Код сообщения об ошибке.
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Текст сообщения об ошибке.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Дополнительная информация, связанная с ошибкой
        /// </summary>
        public Dictionary<string, string> Info { get; set; }
    }
}