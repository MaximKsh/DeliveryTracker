using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AcceptInvitationRequestViewModel
    {
        /// <summary>
        /// Код приглашения.
        /// </summary>
        [Required(ErrorMessage = "InvitationCode is required")]
        public string InvitationCode { get; set; }
        
        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        [Required(ErrorMessage = "DisplayableName is required")]
        public string DisplayableName { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}