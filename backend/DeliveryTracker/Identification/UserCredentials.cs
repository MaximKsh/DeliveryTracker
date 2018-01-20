using System;

namespace DeliveryTracker.Identification
{
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
        
        public Guid Id { get; }

        public string Code { get; }
        
        public string Role { get; }
        
        public Guid InstanceId { get; }

        public bool Valid =>
            this.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(this.Code)
            && this.InstanceId != Guid.Empty;
    }
}