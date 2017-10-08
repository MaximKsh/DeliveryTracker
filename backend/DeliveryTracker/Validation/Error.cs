using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeliveryTracker.Validation
{
    
    public class Error: IError
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyInfo = 
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        
        public Error(
            string code,
            string message,
            IDictionary<string, string> info = null)
        {
            this.Code = code;
            this.Message = message;
            this.Info = info != null
                ? new ReadOnlyDictionary<string, string>(info)
                : EmptyInfo;
        }
        
        /// <inheritdoc />
        public string Code { get; }
        
        /// <inheritdoc />
        public string Message { get; }
        
        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> Info { get; }
    }
}