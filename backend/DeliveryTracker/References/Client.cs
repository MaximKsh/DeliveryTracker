using System.Collections.Generic;

namespace DeliveryTracker.References
{
    public class Client : ReferenceEntityBase
    {
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
        /// Номер телефона.
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Список адресов.
        /// </summary>
        public IList<Address> Addresses { get; set; }
    }
}