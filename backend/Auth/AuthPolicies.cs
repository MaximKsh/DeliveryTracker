using DeliveryTracker.Roles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryTracker.Auth
{
    /// <summary>
    /// Класс представляет политики для доступа к определенным методам API
    /// </summary>
    public static class AuthPolicies
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
                .RequireRole(RoleInfo.Creator)
                .Build();
        
        public static readonly AuthorizationPolicy ManagerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(RoleInfo.Manager)
                .Build();
        
        public static readonly AuthorizationPolicy CreatorOrManagerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(RoleInfo.Manager, RoleInfo.Creator)
                .Build();
        
        public static readonly AuthorizationPolicy PerformerPolicy =
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole(RoleInfo.Performer)
                .Build();

    }
}