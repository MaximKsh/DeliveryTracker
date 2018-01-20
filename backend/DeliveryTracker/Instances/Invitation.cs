using System;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class Invitation
    {
        /// <summary>
        /// ID приглашения.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode { get; set; }
        
        /// <summary>
        /// Пользователь, создавший приглашение
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Дата создания приглашения.
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime Expires { get; set; }
        
        /// <summary>
        /// ID инстанса, к которому относится приглашение.
        /// </summary>
        public Guid InstanceId { get; set; }
        
        /// <summary>
        /// Роль, на которую назначено приглашение.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Предварительные данные о пользователе, указанные при создании приглашения.
        /// </summary>
        public User PreliminaryUser { get; set; }

    }
}