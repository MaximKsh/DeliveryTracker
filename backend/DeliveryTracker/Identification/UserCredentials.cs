using System;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Учетные данные пользователя.
    /// </summary>
    public sealed class UserCredentials
    {
        public UserCredentials(User user)
        {
            this.Id = user.Id;
            this.Code = user.Code;
            this.Role = user.Role;
            this.InstanceId = user.InstanceId;
        }
        
        public UserCredentials(
            Guid id,
            string code,
            string role,
            Guid instanceId)
        {
            this.Id = id;
            this.Code = code;
            this.Role = role;
            this.InstanceId = instanceId;
        }
        
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Код пользователя.
        /// </summary>
        public string Code { get; }
        
        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public string Role { get; }
        
        /// <summary>
        /// ID инстанса пользователя.
        /// </summary>
        public Guid InstanceId { get; }

        
        /// <summary>
        /// Содержит ли объект корректные данные о пользователе.
        /// </summary>
        public bool Valid =>
            this.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(this.Code)
            && this.InstanceId != Guid.Empty;
    }
}