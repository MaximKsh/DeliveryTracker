using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class User : IDictionarySerializable
    {
        /// <summary>
        /// Уникальное имя пользователя(код приглашения).
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Фамилия.
        /// </summary>
        public string Surname { get; set; }
        
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Телефон пользователя.
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Инстанс пользователя.
        /// </summary>
        public Instance Instance { get; set; }
        
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Текущая позиция. Актуально только для исполнителя.
        /// </summary>
        public Geoposition Position { get; set; }

        public IDictionary<string, object> Serialize()
        {
            throw new System.NotImplementedException();
        }

        public void Deserialize(IDictionary<string, object> dict)
        {
            throw new System.NotImplementedException();
        }
    }
}