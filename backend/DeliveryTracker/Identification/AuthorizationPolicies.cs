﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Класс представляет политики для доступа к определенным методам API
    /// </summary>
    public static class AuthorizationPolicies
    {
        public const string Creator = "CreatorPolicy";

        public const string Manager = "ManagerPolicy";
        
        public const string CreatorOrManager = "CreatorOrManagerPolicy";

        public const string Performer = "PerformerPolicy";

        public static readonly AuthorizationPolicy DefaultPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        
        public static readonly AuthorizationPolicy CreatorPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(DeliveryTrackerClaims.Roles, DefaultRoles.CreatorRole)
                .Build();
        
        public static readonly AuthorizationPolicy ManagerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(DeliveryTrackerClaims.Roles, DefaultRoles.ManagerRole)
                .Build();
        
        public static readonly AuthorizationPolicy CreatorOrManagerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(DeliveryTrackerClaims.Roles, DefaultRoles.CreatorRole, DefaultRoles.ManagerRole)
                .Build();
        
        public static readonly AuthorizationPolicy PerformerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(DeliveryTrackerClaims.Roles, DefaultRoles.PerformerRole)
                .Build();

    }
}