using System;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Instances
{
    public class Invitation : DictionaryObject
    {
        /// <summary>
        /// ID приглашения.
        /// </summary>
        public Guid Id 
        {
            get => this.Get<Guid>(nameof(this.Id));
            set => this.Set(nameof(this.Id), value);
        }
        
        /// <summary>
        /// Код приглашения.
        /// </summary>
        public string InvitationCode
        {
            get => this.Get<string>(nameof(this.InvitationCode));
            set => this.Set(nameof(this.InvitationCode), value);
        }
        
        /// <summary>
        /// Пользователь, создавший приглашение
        /// </summary>
        public Guid CreatorId
        {
            get => this.Get<Guid>(nameof(this.CreatorId));
            set => this.Set(nameof(this.CreatorId), value);
        }

        /// <summary>
        /// Дата создания приглашения.
        /// </summary>
        public DateTime Created 
        {
            get => this.Get<DateTime>(nameof(this.Created));
            set => this.Set(nameof(this.Created), value);
        }
        
        /// <summary>
        /// Дата истечения кода.
        /// </summary>
        public DateTime Expires
        {
            get => this.Get<DateTime>(nameof(this.Expires));
            set => this.Set(nameof(this.Expires), value);
        }
        
        /// <summary>
        /// ID инстанса, к которому относится приглашение.
        /// </summary>
        public Guid InstanceId 
        {
            get => this.Get<Guid>(nameof(this.InstanceId));
            set => this.Set(nameof(this.InstanceId), value);
        }
        
        /// <summary>
        /// Роль, на которую назначено приглашение.
        /// </summary>
        public Guid Role 
        {
            get => this.Get<Guid>(nameof(this.Role));
            set => this.Set(nameof(this.Role), value);
        }

        /// <summary>
        /// Предварительные данные о пользователе, указанные при создании приглашения.
        /// </summary>
        public User PreliminaryUser 
        {
            get => this.GetObject<User>(nameof(this.PreliminaryUser));
            set => this.Set(nameof(this.PreliminaryUser), value);
        }

    }
}