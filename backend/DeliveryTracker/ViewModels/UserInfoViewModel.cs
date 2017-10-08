using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserInfoViewModel
    {
        /// <summary>
        /// Уникальное имя пользователя(код приглашения).
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Отображаемое имя пользователя.
        /// </summary>
        public string DisplayableName { get; set; }
        
        /// <summary>
        /// Группа пользователя.
        /// </summary>
        public string Group { get; set; }
        
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