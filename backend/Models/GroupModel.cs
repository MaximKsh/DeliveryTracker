using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.Models
{
    /// <summary>
    /// Группа, объединяющая управляющих и исполнителей.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class GroupModel
    {
        /// <summary>
        /// ID группы.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Отображаемое имя группы.
        /// </summary>
        public string DisplayableName { get; set; }

        /// <summary>
        /// Идентификатор создателя группы
        /// </summary>
        public Guid? CreatorId { get; set; }
        
        /// <summary>
        /// Создать группы.
        /// </summary>
        public virtual UserModel Creator { get; set; }

        /// <summary>
        /// Пользователи группы.
        /// </summary>
        public virtual ICollection<UserModel> Users { get; set; }
        
        /// <summary>
        /// Приглашения в группу.
        /// </summary>
        public virtual ICollection<InvitationModel> Invitations { get; set; }
    }
}