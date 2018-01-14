using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    public class CodePassword : IDictionarySerializable
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Code)] = this.Code,
                [nameof(this.Password)] = this.Password,
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Code = dict.GetPlain<string>(nameof(this.Code));
            this.Password = dict.GetPlain<string>(nameof(this.Password));
        }
    }
}