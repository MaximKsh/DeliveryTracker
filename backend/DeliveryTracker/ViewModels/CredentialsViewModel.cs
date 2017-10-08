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
        /// Имя пользователя (указанное создателем или код приглашения).
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.UserNameIsRequired)]
        public string UserName { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.PasswordIsRequired)]
        public string Password { get; set; }
        
        /// <summary>
        /// Какая роль ожидается для пользователя.
        /// </summary>
        [Required(ErrorMessage =  LocalizationString.Error.RoleIsRequired)]
        [RegularExpression(
            "^(" + RoleInfo.Creator + "|"+ RoleInfo.Manager
              + "|" + RoleInfo.Performer  + ")$",
            ErrorMessage = "Role must be CreatorRole, ManagerRole, PerformerRole")]
        public string Role { get; set; }
    }
}