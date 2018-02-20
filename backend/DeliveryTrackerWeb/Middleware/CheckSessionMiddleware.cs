using System;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using Microsoft.AspNetCore.Http;

namespace DeliveryTrackerWeb.Middleware
{
    public class CheckSessionMiddleware
    {
        private static readonly string AuthHeader = HttpRequestHeader.Authorization.ToString();
        
        private readonly RequestDelegate next;

        public CheckSessionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ISecurityManager securityManager)
        {
            if (!context.Request.Headers.ContainsKey(AuthHeader))
            {
                await this.next(context);
                return;
            }
            
            var id = Guid.Empty;
            var sessionId = Guid.Empty;
            foreach (var claim in  context.User.Claims)
            {
                switch (claim.Type)
                {
                    case DeliveryTrackerClaims.Id when Guid.TryParse(claim.Value, out var uid):
                        id = uid;
                        break;
                    case DeliveryTrackerClaims.TokenId when Guid.TryParse(claim.Value, out var sid):
                        sessionId = sid;
                        break;
                }
            }

            var hasSession = await securityManager.HasSession(id, sessionId);

            if (hasSession.Success)
            {
                await this.next(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}