using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UsernamePassword : DictionarySerializableBase
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }

        /// <inheritdoc />
        public override IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Username)] = this.Username,
                [nameof(this.Password)] = this.Password,
            };
        }

        /// <inheritdoc />
        public override void Deserialize(IDictionary<string, object> dict)
        {
            this.Username = GetPlain<string>(dict, nameof(this.Username));
            this.Password = GetPlain<string>(dict, nameof(this.Password));
        }
    }
}