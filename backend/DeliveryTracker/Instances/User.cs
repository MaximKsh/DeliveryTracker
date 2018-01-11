using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;
using DeliveryTracker.DbModels;

namespace DeliveryTracker.Instances
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class User : DictionarySerializableBase
    {
        public User()
        {

        }

        public User(UserModel userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException();
            }

            this.Username = userModel.UserName;
            this.Surname = userModel.Surname;
            this.Name = userModel.Name;
            this.PhoneNumber = userModel.PhoneNumber;

            if (userModel.Instance != null)
            {
                this.Instance = new Instance(userModel.Instance);
            }

            if (userModel.Latitude != null 
                && userModel.Longitude != null)
            {
                this.Position = new Geoposition
                {
                    Latitude = userModel.Latitude.Value,
                    Longitude = userModel.Longitude.Value,
                };
            }
        }

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
                [nameof(this.Username)] = this.Username,
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
            this.Username = GetPlain<string>(dict, nameof(this.Username));
            this.Surname = GetPlain<string>(dict, nameof(this.Surname));
            this.Name = GetPlain<string>(dict, nameof(this.Name));
            this.PhoneNumber = GetPlain<string>(dict, nameof(this.PhoneNumber));
            this.Role = GetPlain<string>(dict, nameof(this.Role));
            this.Instance = GetObject(this.Instance, dict, nameof(this.Instance));
            this.Position = GetObject(this.Position, dict, nameof(this.Position));
        }
    }
}