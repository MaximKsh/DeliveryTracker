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
        /// Какая роль ожидается для пользователя.
        /// </summary>
        [RegularExpression(
            "^(" + RoleInfo.Creator + "|"+ RoleInfo.Manager
              + "|" + RoleInfo.Performer  + ")$",
            ErrorMessage = LocalizationString.Error.RoleRange)]
        public string Role { get; set; }
    }
}