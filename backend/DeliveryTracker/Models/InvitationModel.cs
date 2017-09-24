using System;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.Models
{
    /// <summary>
    /// Приглашение.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InvitationModel
    {
        /// <summary>
        /// ID приглашения.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode {get; set; }

        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Идентификатор роли, для которой предназначено приглашение.
        /// </summary>
        public Guid RoleId { get; set; }
        
        /// <summary>
        /// Роль, для которой предназначено приглашение.
        /// </summary>
        public virtual RoleModel Role { get; set; }
        
        /// <summary>
        /// Идентификатор группы, в которую приглашают.
        /// </summary>
        public Guid GroupId { get; set; }
        
        /// <summary>
        /// Группа, в которую приглашают.
        /// </summary>
        public virtual GroupModel Group { get; set; }
    }
}