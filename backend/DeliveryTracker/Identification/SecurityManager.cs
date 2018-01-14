using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public class SecurityManager : ISecurityManager
    {
        #region sql
        
        private const string SqlSelectPassword = @"
select
    id,
    instance_id,
    role,
    password_hash
from users
where code = @code
;";

        private const string SqlUpdatePassword = @"
update users
set password_hash = @password
where id = @id
returning code, role, instance_id
;";

        #endregion
        
        #region fields
        
        public const string AuthType = "Token";

        private readonly IPostgresConnectionProvider cp;

        private readonly TokenSettings tokenSettings;

        private readonly PasswordSettings passwordSettings;
        
        private readonly PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        #endregion
        
        #region constructor
        
        public SecurityManager(
            IPostgresConnectionProvider cp,
            TokenSettings tokenSettings,
            PasswordSettings passwordSettings)
        {
            this.cp = cp;
            this.tokenSettings = tokenSettings;
            this.passwordSettings = passwordSettings;
        }

        #endregion
        
        #region public
        
        /// <inheritdoc />
        public async Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            string code,
            string password,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            using (var connWrapper = outerConnection ?? this.cp.Create())
            {
                connWrapper.Connect();
                
                Guid id;
                Guid instanceId;
                var role = string.Empty;
                var passwordHash = string.Empty;
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlSelectPassword;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            id = reader.GetGuid(0);
                            instanceId = reader.GetGuid(1);
                            role = reader.GetString(2);
                            passwordHash = reader.GetString(3);
                        }
                    }
                }
                if (id != Guid.Empty
                    && this.ComparePasswords(passwordHash, password))
                {
                    var credentials = new UserCredentials(id, code, role, instanceId);
                    return new ServiceResult<UserCredentials>(credentials);
                }

                return new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<UserCredentials>> SetPasswordAsync(
            Guid userId, 
            string newPassword,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            if (!this.CheckPassword(newPassword, out var error))
            {
                return new ServiceResult<UserCredentials>(error);
            }
            var newHashedPassword = this.passwordHasher.HashPassword(null, newPassword);
            using (var connWrapper = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                Guid instanceId;
                string code;
                string role;
                
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlUpdatePassword;
                    command.Parameters.Add(new NpgsqlParameter("password", newHashedPassword));
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            code = reader.GetString(0);
                            role = reader.GetString(1);
                            instanceId = reader.GetGuid(2);
                        }
                        else
                        {
                            return new ServiceResult<UserCredentials>(ErrorFactory.UserNotFound(userId));
                        }
                    }
                }
                var credentials = new UserCredentials(userId, code, role, instanceId);
                return new ServiceResult<UserCredentials>(credentials);
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
        
        #endregion

        #region private
        
        private bool ComparePasswords(string hash, string newPassword) =>
            this.passwordHasher.VerifyHashedPassword(null, hash, newPassword) == PasswordVerificationResult.Success;

        private bool CheckPassword(string password, out IError error)
        {
            error = null;
            var hasUpperCase = false;
            var hasLowerCase = false;
            var hasOneDigit = false;
            var forbiddenCharacters = new char[this.passwordSettings.MaxLength];
            var maxSameCharactersInARow = 0;
            var currentSameCharactersInARow = 0;
            var len = 0;
            var forbiddentCharactedIdx = 0;
            var previousCh = char.MinValue;
            foreach (var ch in password)
            {
                // Проверка на наличие только разрешенных символов
                if (this.passwordSettings.HasAlphabet
                    && !this.passwordSettings.Alphabet.Contains(ch))
                {
                    forbiddenCharacters[forbiddentCharactedIdx] = ch;
                    forbiddentCharactedIdx++;
                }
                // Проверка на наличие верхнего регистра
                else if (!hasUpperCase 
                    && char.IsUpper(ch))
                {
                    hasUpperCase = true;
                }
                // Проверка на наличие нижнего регистра
                else if (!hasLowerCase 
                         && char.IsLower(ch))
                {
                    hasLowerCase = true;
                }
                // Проверка на наличие цифры
                else if (!hasOneDigit 
                         && char.IsDigit(ch))
                {
                    hasOneDigit = true;
                }

                // Проверка на повторяющийся символ
                if (previousCh == ch)
                {
                    currentSameCharactersInARow++;
                    if (currentSameCharactersInARow > maxSameCharactersInARow)
                    {
                        maxSameCharactersInARow = currentSameCharactersInARow;
                    }
                }
                else
                {
                    currentSameCharactersInARow = 0;
                }
                if (len > this.passwordSettings.MaxLength)
                {
                    break;
                }

                previousCh = ch;
                len++;
            }

            var internalError = ErrorFactory.IncorrectPassword(
                len < this.passwordSettings.MinLength ? (int?) this.passwordSettings.MinLength : null,
                len > this.passwordSettings.MaxLength ? (int?) this.passwordSettings.MaxLength : null,
                this.passwordSettings.AtLeastOneUpperCase && !hasUpperCase,
                this.passwordSettings.AtLeastOneLowerCase && !hasLowerCase,
                this.passwordSettings.AtLeastOneDigit && !hasOneDigit,
                forbiddentCharactedIdx != 0 ? new string(forbiddenCharacters) : null,
                maxSameCharactersInARow > this.passwordSettings.SameCharactersInARow
                    ? (int?) this.passwordSettings.SameCharactersInARow
                    : null);
            if (internalError.Info.Count == 0)
            {
                return true;
            }
            error = internalError;
            return false;

        }

        #endregion
    }
}