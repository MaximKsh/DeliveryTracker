using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryTracker.Models
{
    /// <summary>
    /// Инстанс, объединяющая управляющих и исполнителей.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class InstanceModel
    {
        /// <summary>
        /// ID инстанса.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Отображаемое имя инстанса.
        /// </summary>
        public string DisplayableName { get; set; }

        /// <summary>
        /// Идентификатор создателя инстанса
        /// </summary>
        public Guid? CreatorId { get; set; }
        
        /// <summary>
        /// Создать инстанса.
        /// </summary>
        public virtual UserModel Creator { get; set; }

        /// <summary>
        /// Пользователи инстанса.
        /// </summary>
        public virtual ICollection<UserModel> Users { get; set; }
        
        /// <summary>
        /// Приглашения в инстанса.
        /// </summary>
        public virtual ICollection<InvitationModel> Invitations { get; set; }
    }
}