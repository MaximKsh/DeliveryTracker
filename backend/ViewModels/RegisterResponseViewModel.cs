using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class RegisterResponseViewModel
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
    }
}