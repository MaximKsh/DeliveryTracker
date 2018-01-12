using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using DeliveryTracker.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public class SecurityManager : ISecurityManager
    {
        private const string SqlSelectPassword = @"
select
    id,
    instance_id,
    password_hash
from users
where code = @code
;";

        private const string SqlUpdatePassword = @"
update users
set password_hash = @password
where id = @id
;";

        public const string AuthType = "Token";

        private readonly IPostgresConnectionProvider cp;

        private readonly TokenSettings tokenSettings;

        private readonly PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        public SecurityManager(IPostgresConnectionProvider cp, TokenSettings tokenSettings)
        {
            this.tokenSettings = tokenSettings;
            this.cp = cp;
        }

        /// <inheritdoc />
        public async Task<UserCredentials> ValidatePasswordAsync(
            string code,
            string password,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            using (var connWrapper = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                var conn = connWrapper.Connection;
                Guid id;
                Guid instanceId;
                var passwordHash = string.Empty;
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlSelectPassword;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            id = reader.GetGuid(0);
                            instanceId = reader.GetGuid(1);
                            passwordHash = reader.GetString(2);
                        }
                    }
                }
                if (id != Guid.Empty
                    && this.CheckPassword(passwordHash, password))
                {
                    return new UserCredentials(id, code, "", instanceId);
                }

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetPasswordAsync(
            Guid userId, 
            string newPassword,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            var newHashedPassword = this.passwordHasher.HashPassword(null, newPassword);
            using (var connWrapper = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                var conn = connWrapper.Connection;
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlUpdatePassword;
                    command.Parameters.Add(new NpgsqlParameter("password", newHashedPassword));
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    return await command.ExecuteNonQueryAsync() != 0;
                }
            }
        }

        /// <inheritdoc />
        public string AcquireToken(UserCredentials credentials)
        {
            var claims = new List<Claim>
            {
                new Claim(DeliveryTrackerClaims.Id, credentials.Id.ToString()),
                new Claim(DeliveryTrackerClaims.Code, credentials.Code),
                new Claim(DeliveryTrackerClaims.Roles, credentials.Role),
                new Claim(DeliveryTrackerClaims.InstanceId, credentials.InstanceId.ToString()),
            };

            var identity = new ClaimsIdentity(
                claims,
                AuthType,
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: this.tokenSettings.Issuer,
                audience: this.tokenSettings.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(this.tokenSettings.Lifetime),
                signingCredentials: new SigningCredentials(
                    this.tokenSettings.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private bool CheckPassword(string hash, string newPassword) =>
            this.passwordHasher.VerifyHashedPassword(null, hash, newPassword) == PasswordVerificationResult.Success;
    }
}