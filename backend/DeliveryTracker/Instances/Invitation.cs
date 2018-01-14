using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class Invitation : IDictionarySerializable
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