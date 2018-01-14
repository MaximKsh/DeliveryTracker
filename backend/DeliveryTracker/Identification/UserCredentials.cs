using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryTracker.Identification
{
    public sealed class UserCredentials
    {
        public UserCredentials(
            Guid id,
            string code,
            IEnumerable<Role> roles,
            Guid instanceId)
        {
            this.Id = id;
            this.Code = code;
            this.Roles = roles.ToList().AsReadOnly();
            this.InstanceId = instanceId;
        }
        
        public Guid Id { get; }

        public string Code { get; }
        
        public IReadOnlyList<Role> Roles { get; }
        
        public Guid InstanceId { get; }

        public bool Valid =>
            this.Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(this.Code)
            && this.InstanceId != Guid.Empty;
    }
}