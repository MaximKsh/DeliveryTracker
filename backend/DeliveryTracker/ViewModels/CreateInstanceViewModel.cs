using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    /// <summary>
    /// View-Model запроса на создание группы.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateInstanceViewModel
    {
        /// <summary>
        /// Информация о создаваемом инстансе.
        /// </summary>
        public InstanceViewModel Instance { get; set; }
        
        /// <summary>
        /// Пароль для нового аккаунта создателя группы.
        /// </summary>
        public CredentialsViewModel Credentials { get; set; }
        
        /// <summary>
        /// Информация о создателе группы.
        /// </summary>
        public UserViewModel Creator { get; set; }
    }
}
