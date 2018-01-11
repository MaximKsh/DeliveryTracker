using System;
using System.Collections.Generic;
using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    public class Invitation : DictionarySerializableBase
    {
        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode { get; set; }

        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Предварительные данные о пользователе, указанные при создании приглашения.
        /// </summary>
        public User PreliminaryUser { get; set; }

        /// <inheritdoc />
        public override IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                [nameof(this.InvitationCode)] = this.InvitationCode,
                [nameof(this.ExpirationDate)] = this.ExpirationDate,
                [nameof(this.PreliminaryUser)] = this.PreliminaryUser.Serialize(),
            };
        }

        /// <inheritdoc />
        public override void Deserialize(IDictionary<string, object> dict)
        {
            this.InvitationCode = GetPlain<string>(dict, nameof(this.InvitationCode));
            this.ExpirationDate = GetPlain<DateTime>(dict, nameof(this.ExpirationDate));
            this.PreliminaryUser = GetObject(this.PreliminaryUser, dict, nameof(this.InvitationCode));
        }
    }
}