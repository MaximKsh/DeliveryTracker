using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class Invitation : IDictionarySerializable
    {
        public Invitation()
        {
        }

        public Invitation(
            Guid id,
            string invitationCode,
            DateTime expirationDate,
            Guid instanceId,
            Guid roleId)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }
        
        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode { get; private set; }

        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime ExpirationDate { get; private set; }
        
        /// <summary>
        /// ID инстанса, к которому относится приглашение.
        /// </summary>
        public Guid InstanceId { get; private set; }
        
        /// <summary>
        /// Роль, на которую назначено приглашение.
        /// </summary>
        public Guid RoleId { get; private set; }

        /// <summary>
        /// Предварительные данные о пользователе, указанные при создании приглашения.
        /// </summary>
        public User PreliminaryUser { get; private set; }

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.InvitationCode)] = this.InvitationCode,
                [nameof(this.ExpirationDate)] = this.ExpirationDate,
                [nameof(this.PreliminaryUser)] = this.PreliminaryUser.Serialize(),
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.InvitationCode = dict.GetPlain<string>(nameof(this.InvitationCode));
            this.ExpirationDate = dict.GetPlain<DateTime>(nameof(this.ExpirationDate));
            this.PreliminaryUser = dict.GetObject<User>(nameof(this.PreliminaryUser));
        }
    }
}