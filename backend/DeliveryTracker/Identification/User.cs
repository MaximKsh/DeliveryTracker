using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;
using DeliveryTracker.Instances;

namespace DeliveryTracker.Identification
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class User : DictionarySerializableBase
    {
        /// <summary>
        /// ID пользователя.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Код пользователя.
        /// </summary>
        public string Code { get; set; }
        
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
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Инстанс пользователя.
        /// </summary>
        public Instance Instance { get; set; }
        
        /// <summary>
        /// Текущая позиция. Актуально только для исполнителя.
        /// </summary>
        public Geoposition Position { get; set; }

        /// <inheritdoc />
        public override IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.Code)] = this.Code,
                [nameof(this.Surname)] = this.Surname,
                [nameof(this.Name)] = this.Name,
                [nameof(this.PhoneNumber)] = this.PhoneNumber,
                [nameof(this.Role)] = this.Role,
                [nameof(this.Instance)] = this.Instance,
                [nameof(this.Position)] = this.Position.Serialize(),
            };
        }

        /// <inheritdoc />
        public override void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = GetPlain<Guid>(dict, nameof(this.Id));
            this.Code = GetPlain<string>(dict, nameof(this.Code));
            this.Surname = GetPlain<string>(dict, nameof(this.Surname));
            this.Name = GetPlain<string>(dict, nameof(this.Name));
            this.PhoneNumber = GetPlain<string>(dict, nameof(this.PhoneNumber));
            this.Role = GetPlain<string>(dict, nameof(this.Role));
            this.Instance = GetObject(this.Instance, dict, nameof(this.Instance));
            this.Position = GetObject(this.Position, dict, nameof(this.Position));
        }
    }
}