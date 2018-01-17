using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeliveryTracker.Validation
{
    /// <inheritdoc />True
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

        public bool Equals(IError other)
        {
            return string.Equals(this.Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() 
                   && this.Equals((Error) obj);
        }

        public override int GetHashCode()
        {
            return (this.Code != null ? this.Code.GetHashCode() : 0);
        }
    }
}