using System;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Common;
using DeliveryTracker.Geopositioning;
using Newtonsoft.Json;

namespace DeliveryTracker.Identification
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class User : DictionaryObject
    {
        /// <summary>
        /// ID пользователя.
        /// </summary>
        public Guid Id 
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }

        /// <summary>
        /// Инстанс пользователя.
        /// </summary>
        public Guid InstanceId 
        {
            get => this.Get<Guid>(nameof(this.InstanceId));
            set => this.Set(nameof(this.InstanceId), value);
        }
        
        /// <summary>
        /// Дата последней активности пользователя. Не сериализуется.
        /// </summary>
        [JsonIgnore]
        public DateTime LastActivity { get; set; }
        
        /// <summary>
        /// Код пользователя.
        /// </summary>
        public string Code 
        {
            get => this.Get<string>(nameof(this.Code));
            set => this.Set(nameof(this.Code), value);
        }
        
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public Guid Role 
        {
            get => this.Get<Guid>(nameof(this.Role));
            set => this.Set(nameof(this.Role), value);
        }
        
        /// <summary>
        /// Фамилия.
        /// </summary>
        public string Surname 
        {
            get => this.Get<string>(nameof(this.Surname));
            set => this.Set(nameof(this.Surname), value);
        }
        
        /// <summary>
        /// Имя.
        /// </summary>
        public string Name
        {
            get => this.Get<string>(nameof(this.Name));
            set => this.Set(nameof(this.Name), value);
        }
        
        /// <summary>
        /// Отчество.
        /// </summary>
        public string Patronymic 
        {
            get => this.Get<string>(nameof(this.Patronymic));
            set => this.Set(nameof(this.Patronymic), value);
        }
        
        /// <summary>
        /// Телефон пользователя.
        /// </summary>
        public string PhoneNumber
        {
            get => this.Get<string>(nameof(this.PhoneNumber));
            set => this.Set(nameof(this.PhoneNumber), value);
        }
        
        /// <summary>
        /// Текущая позиция. Актуально только для исполнителя.
        /// </summary>
        public Geoposition Geoposition 
        {
            get => this.GetObject<Geoposition>(nameof(this.Geoposition));
            set => this.Set(nameof(this.Geoposition), value);
        }

        /// <summary>
        /// Пользователь сейчас активен.
        /// </summary>
        [JsonProperty]
        public bool Online
        {
            get
            {
                var online = OnlineChecker.IsOnline(this);
                this.Set(nameof(this.Online), online);
                return online;
            }
        }

        protected override void BeforeGetDictionary()
        {
            this.Set(nameof(this.Online), OnlineChecker.IsOnline(this));
        }
    }
}