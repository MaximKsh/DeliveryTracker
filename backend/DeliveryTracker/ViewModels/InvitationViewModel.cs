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
        /// Дата истечения кода.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.ExpirationDateIsRequired)]
        public DateTime ExpirationDate { get; set; }
        
        /// <summary>
        /// Предварительные данные о пользователе, указанные при создании приглашения.
        /// </summary>
        public UserViewModel PreliminaryUser { get; set; }
    }
}