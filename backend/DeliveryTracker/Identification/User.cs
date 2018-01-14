using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Роли пользователя.
        /// </summary>
        public IReadOnlyList<Role> Roles { get; set; }

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
                [nameof(this.Surname)] = this.Surname,
                [nameof(this.Name)] = this.Name,
                [nameof(this.Patronymic)] = this.Patronymic,
                [nameof(this.PhoneNumber)] = this.PhoneNumber,
                [nameof(this.Roles)] = this.Roles.SerializeObjectList(),
                [nameof(this.Position)] = this.Position?.Serialize(),
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = dict.GetPlain<Guid>(nameof(this.Id));
            this.Code = dict.GetPlain<string>(nameof(this.Code));
            this.Surname = dict.GetPlain<string>(nameof(this.Surname));
            this.Name = dict.GetPlain<string>(nameof(this.Name));
            this.Patronymic = dict.GetPlain<string>(nameof(this.Patronymic));
            this.PhoneNumber = dict.GetPlain<string>(nameof(this.PhoneNumber));
            var mutableRoles = dict.GetObjectList<Role>(nameof(this.Roles));
            this.Roles = mutableRoles != null 
                ? new ReadOnlyCollection<Role>(mutableRoles)
                : null;
            this.Position = dict.GetObject<Geoposition>(nameof(this.Position));
        }
    }
}