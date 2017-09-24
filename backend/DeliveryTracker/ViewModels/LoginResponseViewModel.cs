using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LoginResponseViewModel
    {
        /// <summary>
        /// Отображаемое имя пользователя.
        /// </summary>
        public string DisplayableName { get; set; }
        
        /// <summary>
        /// JWT токен.
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public string Role { get; set; }
    }
}