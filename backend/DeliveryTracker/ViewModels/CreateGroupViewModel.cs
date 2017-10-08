using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    /// <summary>
    /// View-Model запроса на создание группы.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateGroupViewModel
    {
        /// <summary>
        /// Имя группы.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.GroupNameIsRequired)]
        public string GroupName { get; set; }
        
        /// <summary>
        /// Имя создателя группы.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.CreatorDisplayableNameIsRequired)]
        public string CreatorDisplayableName { get; set; }
        
        /// <summary>
        /// Пароль на аккаунт создателя группы.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.CreatorPasswordIsRequired)]
        public string CreatorPassword { get; set; }
    }
}
