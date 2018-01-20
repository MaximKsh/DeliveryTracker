using System;
using Microsoft.AspNetCore.Http;

namespace DeliveryTracker.Identification
{
    public class UserCredentialsAccessor : IUserCredentialsAccessor
    {
        private readonly IHttpContextAccessor ctxAccessor;
        
        public UserCredentialsAccessor(IHttpContextAccessor ctxAccessor)
        {
            this.ctxAccessor = ctxAccessor;
        }

        public UserCredentials UserCredentials => this.CreateFromContext();

        private UserCredentials CreateFromContext()
        {
            var ctx = this.ctxAccessor.HttpContext;
            var id = Guid.Empty;
            var code = string.Empty;
            var instanceId = Guid.Empty;
            var role = string.Empty;
            foreach (var claim in ctx.User.Claims)
            {
                if (claim.Type == DeliveryTrackerClaims.Id
                    && Guid.TryParse(claim.Value, out var uid))
                {
                    id = uid;
                }
                else if (claim.Type == DeliveryTrackerClaims.Code)
                {
                    code = claim.Value;
                }
                else if (claim.Type == DeliveryTrackerClaims.InstanceId
                         && Guid.TryParse(claim.Value, out var iid))
                {
                    instanceId = iid;
                }
                else if (claim.Type == DeliveryTrackerClaims.Role)
                {
                    role = claim.Value;
                }
            }
            
            return new UserCredentials(
                id,
                code,
                role,
                instanceId);
        }
    }
}