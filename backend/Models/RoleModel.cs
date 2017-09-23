using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace DeliveryTracker.Models
{
    /// <summary>
    /// Роль пользователя.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class RoleModel: IdentityRole<Guid>
    {
        /// <summary>
        /// Список приглашений на данную роль.
        /// </summary>
        public virtual ICollection<InvitationModel> Invitations { get; set; }
    }
}