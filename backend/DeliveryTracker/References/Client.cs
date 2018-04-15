using System.Collections.Generic;

namespace DeliveryTracker.References
{
    public sealed class Client : ReferenceEntryBase
    {
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
        /// Номер телефона.
        /// </summary>
        public string PhoneNumber 
        {
            get => this.Get<string>(nameof(this.PhoneNumber));
            set => this.Set(nameof(this.PhoneNumber), value);
        }
        
        /// <summary>
        /// Список адресов.
        /// </summary>
        public IList<ClientAddress> Addresses 
        {
            get => this.GetList<ClientAddress>(nameof(this.Addresses));
            set => this.Set(nameof(this.Addresses), value);
        }
    }
}