using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InvitationViewModel
    {
        /// <summary>
        /// Код приглашения.
        /// </summary>
        [Required(ErrorMessage =  LocalizationString.Error.InvitationCodeIsRequired)]
        public string InvitationCode { get; set; }
        
        /// <summary>
        /// Роль, для которой действителен код приглашения.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.RoleIsRequired)]
        public string Role { get; set; }
        
        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.ExpirationDateIsRequired)]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Название группы в которую приглашают.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.GroupNameIsRequired)]
        public string GroupName { get; set; }
    }
}