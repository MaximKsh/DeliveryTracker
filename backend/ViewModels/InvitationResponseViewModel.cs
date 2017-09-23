using System;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InvitationResponseViewModel
    {
        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode { get; set; }
        
        /// <summary>
        /// Роль, для которой действителен код приглашения.
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime ExpirationDate { get; set; }

    }
}