using System;
using System.Linq;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Учетные данные пользователя.
    /// </summary>
    public sealed class UserCredentials : IEquatable<UserCredentials>
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
            Guid role,
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
        /// ID роли пользователя.
        /// </summary>
        public Guid Role { get; }
        
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
            && DefaultRoles.AllRoles.Contains(this.Role)
            && this.InstanceId != Guid.Empty;

        
        public bool Equals(
            UserCredentials other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return this.Id.Equals(other.Id) 
                   && string.Equals(this.Code, other.Code) 
                   && string.Equals(this.Role, other.Role) 
                   && this.InstanceId.Equals(other.InstanceId);
        }

        public override bool Equals(
            object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is UserCredentials credentials 
                   && this.Equals(credentials);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Code != null ? this.Code.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Role.GetHashCode());
                hashCode = (hashCode * 397) ^ this.InstanceId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(
            UserCredentials left,
            UserCredentials right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            UserCredentials left,
            UserCredentials right)
        {
            return !Equals(left, right);
        }
    }
}