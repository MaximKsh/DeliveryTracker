using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AcceptInvitationViewModel
    {
        /// <summary>
        /// Код приглашения.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.InvitationCodeIsRequired)]
        public string InvitationCode { get; set; }
        
        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.DisplayableNameIsRequired)]
        public string DisplayableName { get; set; }
        
        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.PasswordIsRequired)]
        public string Password { get; set; }
    }
}