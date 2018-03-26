using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    public class Device : DictionaryObject
    {
        /// <summary>
        /// ID пользователя.
        /// </summary>
        public Guid UserId 
        {
            get => this.Get<Guid>(nameof(this.UserId));
            set => this.Set(nameof(this.UserId), value);
        }

        /// <summary>
        /// Тип устройства.
        /// </summary>
        public string Type
        {
            get => this.Get<string>(nameof(this.Type));
            set => this.Set(nameof(this.Type), value);
        }

        /// <summary>
        /// Версия устройства.
        /// </summary>
        public string Version
        {
            get => this.Get<string>(nameof(this.Version));
            set => this.Set(nameof(this.Version), value);
        }
        
        /// <summary>
        /// Тип приложения.
        /// </summary>
        public string ApplicationType
        {
            get => this.Get<string>(nameof(this.ApplicationType));
            set => this.Set(nameof(this.ApplicationType), value);
        }
        
        /// <summary>
        /// Версия приложения.
        /// </summary>
        public string ApplicationVersion
        {
            get => this.Get<string>(nameof(this.ApplicationVersion));
            set => this.Set(nameof(this.ApplicationVersion), value);
        }
        
        /// <summary>
        /// Язык, включенный на устройстве.
        /// </summary>
        public string Language
        {
            get => this.Get<string>(nameof(this.Language));
            set => this.Set(nameof(this.Language), value);
        }
        
        /// <summary>
        /// FirebaseId
        /// </summary>
        public string FirebaseId
        {
            get => this.Get<string>(nameof(this.FirebaseId));
            set => this.Set(nameof(this.FirebaseId), value);
        }
        
    }
}