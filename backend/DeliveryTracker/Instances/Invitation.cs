using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class Invitation : IDictionarySerializable
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

        /// <inheritdoc />
        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.Id)] = this.Id,
                [nameof(this.InvitationCode)] = this.InvitationCode,
                [nameof(this.Created)] = this.Created,
                [nameof(this.Expires)] = this.Expires,
                [nameof(this.InstanceId)] = this.InstanceId,
                [nameof(this.Role)] = this.Role,
                [nameof(this.PreliminaryUser)] = this.PreliminaryUser.Serialize(),
            };
        }

        /// <inheritdoc />
        public void Deserialize(IDictionary<string, object> dict)
        {
            this.Id = dict.GetPlain<Guid>(nameof(this.Id));
            this.InvitationCode = dict.GetPlain<string>(nameof(this.InvitationCode));
            this.Created = dict.GetPlain<DateTime>(nameof(this.Created));
            this.Expires = dict.GetPlain<DateTime>(nameof(this.Expires));
            this.InstanceId = dict.GetPlain<Guid>(nameof(this.InstanceId));
            this.Role = dict.GetPlain<string>(nameof(this.Role));
            this.PreliminaryUser = dict.GetObject<User>(nameof(this.PreliminaryUser));
        }
    }
}