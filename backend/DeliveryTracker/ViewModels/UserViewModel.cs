using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserViewModel
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
        public InstanceViewModel Instance { get; set; }
        
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Текущая позиция. Актуально только для исполнителя.
        /// </summary>
        public GeopositionViewModel Position { get; set; }
    }
}