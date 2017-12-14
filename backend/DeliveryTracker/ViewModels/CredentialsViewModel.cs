using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;
using DeliveryTracker.Roles;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CredentialsViewModel
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Устройство пользователя.
        /// </summary>
        public DeviceViewModel Device { get; set; }
    }
}