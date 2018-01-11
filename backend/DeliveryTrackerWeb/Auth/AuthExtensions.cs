using System;
using System.Collections.Generic;
using System.Security.Claims;
using DeliveryTracker.Instances;
using DeliveryTracker.ViewModels;
using DeliveryTracker.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryTrackerWeb.Auth
{
    public static class AuthExtensions
    {
        public static UserCredentials ToUserCredentials(this IEnumerable<Claim> claims)
        {
            string userName = null;
            string role = null;
            Guid instanceId = Guid.Empty;
            
            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case ClaimsIdentity.DefaultNameClaimType:
                        userName = claim.Value;
                        break;
                    case ClaimsIdentity.DefaultRoleClaimType:
                        role = claim.Value;
                        break;
                }
            }
            
            return new UserCredentials(
                userName,
                role,
                instanceId);
        }
        
        public static IServiceCollection AddDeliveryTrackerAuth(this IServiceCollection services)
        {
            services
                .AddSingleton<ITokenProvider, TokenProvider>()
                ;
            
            return services;
        }
    }
}