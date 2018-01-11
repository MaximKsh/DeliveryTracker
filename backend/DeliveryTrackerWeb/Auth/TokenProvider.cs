using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DeliveryTracker.Instances;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTrackerWeb.Auth
{
    public class TokenProvider : ITokenProvider
    {
        private const string AuthType = "Token";
        
        private readonly AuthInfo authInfo;
        
        public TokenProvider(AuthInfo authInfo)
        {
            this.authInfo = authInfo;
        }
        
        
        public string CreateToken(UserCredentials userCredentials)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userCredentials.UserName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, userCredentials.Role),
                new Claim(DeliveryTrackerClaims.InstanceId, userCredentials.InstanceId.ToString()),
            };
            
            var identity = new ClaimsIdentity(
                claims,
                AuthType,
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: this.authInfo.Issuer,
                audience: this.authInfo.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(this.authInfo.Lifetime),
                signingCredentials: new SigningCredentials(
                    this.authInfo.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}