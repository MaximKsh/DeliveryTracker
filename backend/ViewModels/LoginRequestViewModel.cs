using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Roles;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LoginRequestViewModel
    {
        /// <summary>
        /// Имя пользователя (указанное создателем или код приглашения).
        /// </summary>
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        
        /// <summary>
        /// Какая роль ожидается для пользователя.
        /// </summary>
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression(
            "^(" + RoleInfo.Creator + "|"+ RoleInfo.Manager
              + "|" + RoleInfo.Performer  + ")$",
            ErrorMessage = "Role must be CreatorRole, ManagerRole, PerformerRole")]
        public string Role { get; set; }
    }
}