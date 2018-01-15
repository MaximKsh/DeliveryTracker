using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class User : IDictionarySerializable
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
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Фамилия.
        /// </summary>
        public string Surname { get; set; }
        
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Отчество.
        /// </summary>
        public string Patronymic { get; set; }
        
        /// <summary>
        /// Телефон пользователя.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Инстанс пользователя.
        /// </summary>
        public Guid InstanceId { get; set; }
        
        /// <summary>
        /// Текущая позиция. Актуально только для исполнителя.
        /// </summary>
        public Geoposition Position { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.Code)] = this.Code,
                [nameof(this.Role)] = this.Role,
                [nameof(this.Surname)] = this.Surname,
                [nameof(this.Name)] = this.Name,
                [nameof(this.Patronymic)] = this.Patronymic,
                [nameof(this.PhoneNumber)] = this.PhoneNumber,
                [nameof(this.Position)] = this.Position?.Serialize(),
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = dict.GetPlain<Guid>(nameof(this.Id));
            this.Code = dict.GetPlain<string>(nameof(this.Code));
            this.Role = dict.GetPlain<string>(nameof(this.Role));
            this.Surname = dict.GetPlain<string>(nameof(this.Surname));
            this.Name = dict.GetPlain<string>(nameof(this.Name));
            this.Patronymic = dict.GetPlain<string>(nameof(this.Patronymic));
            this.PhoneNumber = dict.GetPlain<string>(nameof(this.PhoneNumber));
            this.Position = dict.GetObject<Geoposition>(nameof(this.Position));
        }
    }
}