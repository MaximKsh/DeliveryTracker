using System;

namespace DeliveryTracker.Instances
{
    public sealed class UserCredentials
    {
        public UserCredentials(
            string userName,
            string role,
            Guid instanceId)
        {
            this.UserName = userName;
            this.Role = role;
            this.InstanceId = instanceId;
        }
        
        public string UserName { get; }
        
        public string Role { get; }
        
        public Guid InstanceId { get; }

        public bool Valid =>
            !string.IsNullOrWhiteSpace(this.UserName)
            && !string.IsNullOrWhiteSpace(this.Role)
            && this.InstanceId != Guid.Empty;
    }
}