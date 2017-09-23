
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    /// <summary>
    /// View-Model запроса на создание группы.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CreateGroupRequestViewModel
    {
        /// <summary>
        /// Имя группы.
        /// </summary>
        [Required(ErrorMessage = "GroupName is required")]
        public string GroupName { get; set; }
        
        /// <summary>
        /// Имя создателя группы.
        /// </summary>
        [Required(ErrorMessage = "CreatorDisplayableName is required")]
        public string CreatorDisplayableName { get; set; }
        
        /// <summary>
        /// Пароль на аккаунт создателя группы.
        /// </summary>
        [Required(ErrorMessage = "CreatorPassword is required")]
        public string CreatorPassword { get; set; }
    }
}
