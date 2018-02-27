using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
    code,
    instance_id,
    role,
    password_hash
from users
";

        private const string SqlSelectPasswordById = SqlSelectPassword + "where id = @id";
        
        private const string SqlSelectPasswordByCode = SqlSelectPassword + "where code = @code";

        private const string SqlUpdatePassword = @"
update users
set password_hash = @password
where id = @id
returning code, role, instance_id
;";

        private static readonly string SqlUpsertSession = $@"
insert into ""sessions""({IdentificationHelper.GetSessionColumns()})
values ({IdentificationHelper.GetSessionColumns("@")})
on conflict(user_id) do update set
    session_token_id = @session_token_id,
    refresh_token_id = @refresh_token_id,
    last_activity = now() AT TIME ZONE 'UTC'
returning {IdentificationHelper.GetSessionColumns()};
";
        
        private static readonly string SqlHasSessionToken = @"
update ""users""
set ""last_activity"" = now() AT TIME ZONE 'UTC'
where ""id"" = @user_id
;
update ""sessions""
set ""last_activity"" = now() AT TIME ZONE 'UTC'
where ""user_id"" = @user_id
returning ""session_token_id""
; 
";
        
        private static readonly string SqlHasSessionRefreshToken = @"
update ""users""
set ""last_activity"" = now() AT TIME ZONE 'UTC'
where ""id"" = @user_id
;
update ""sessions""
set ""last_activity"" = now() AT TIME ZONE 'UTC'
where ""user_id"" = @user_id
returning ""refresh_token_id""; 
";
        
        #endregion
        
        #region fields
        
        public const string SesssionTokenAuthType = "Token";
        public const string RefreshTokenAuthType = "RefreshToken";

        private readonly IPostgresConnectionProvider cp;

        private readonly IUserCredentialsAccessor accessor;
        
        private readonly TokenSettings sessionTokenSettings;
        
        private readonly TokenSettings refreshTokenSettings;

        private readonly PasswordSettings passwordSettings;
        
        private readonly PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        #endregion
        
        #region constructor
        
        public SecurityManager(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            ISettingsStorage settingsStorage)
        {
            this.cp = cp;
            this.accessor = accessor;
            this.sessionTokenSettings = settingsStorage.GetSettings<TokenSettings>(SettingsName.SessionToken);
            this.refreshTokenSettings = settingsStorage.GetSettings<TokenSettings>(SettingsName.RefreshToken);
            this.passwordSettings = settingsStorage.GetSettings<PasswordSettings>(SettingsName.Password);
        }

        #endregion
        
        #region public
        
        /// <inheritdoc />
        public async Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            string code,
            string password,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
            return await this.ValidatePasswordInternalAsync(
                SqlSelectPasswordByCode,
                new NpgsqlParameter("code", code),
                password,
                outerConnection);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<UserCredentials>> ValidatePasswordAsync(
            Guid userId,
            string password,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
            return await this.ValidatePasswordInternalAsync(
                SqlSelectPasswordById,
                new NpgsqlParameter("id", userId),
                password,
                outerConnection);
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
                Guid role;
                
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
                            role = reader.GetGuid(1);
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
        public async Task<ServiceResult<Session>> NewSessionAsync(
            UserCredentials credentials,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            var sessionToken = NewToken(credentials, this.sessionTokenSettings, out var sessionTokenId);
            var refreshToken = NewToken(credentials, this.refreshTokenSettings, out var refreshTokenId);
            Session session;
            
            using (var conn = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlUpsertSession;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("user_id", credentials.Id));
                    command.Parameters.Add(new NpgsqlParameter("session_token_id", sessionTokenId));
                    command.Parameters.Add(new NpgsqlParameter("refresh_token_id", refreshTokenId));
                    command.Parameters.Add(new NpgsqlParameter("last_activity", DateTime.UtcNow));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();
                        session = reader.GetSession();
                    }

                }
            }

            session.SessionToken = sessionToken;
            session.RefreshToken = refreshToken;
            
            return new ServiceResult<Session>(session);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Session>> RefreshSessionAsync(
            string refreshToken,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            var result = await this.ValidateRefreshTokenAsync(refreshToken, outerConnection);

            return result.Success
                ? await this.NewSessionAsync(result.Result)
                : new ServiceResult<Session>(result.Errors);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> HasSession(
            Guid userId,
            Guid sessionTokenId,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            using (var conn = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlHasSessionToken;
                    command.Parameters.Add(new NpgsqlParameter("user_id", userId));
                    return (await command.ExecuteScalarAsync())?.Equals(sessionTokenId) == true
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.AccessDenied());
                }
            }
        }

        #endregion

        #region private
        
        private bool ComparePasswords(string hash, string newPassword) =>
            this.passwordHasher.VerifyHashedPassword(null, hash, newPassword) == PasswordVerificationResult.Success;

        private bool CheckPassword(string password, out IError error)
        {
            // Если пароль null или пустые символы, отметем как слишком короткий
            if (string.IsNullOrWhiteSpace(password))
            {
                password = string.Empty;
            }
            error = null;
            var hasUpperCase = false;
            var hasLowerCase = false;
            var hasOneDigit = false;
            var forbiddenCharacters = new char[this.passwordSettings.MaxLength];
            var maxSameCharactersInARow = 1;
            var currentSameCharactersInARow = 1;
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
                    currentSameCharactersInARow = 1;
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

        private async Task<ServiceResult<UserCredentials>> ValidatePasswordInternalAsync(
            string sqlScript,
            NpgsqlParameter parameter,
            string password,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            
            using (var connWrapper = outerConnection ?? this.cp.Create())
            {
                connWrapper.Connect();
                
                Guid id;
                Guid instanceId;
                Guid role;
                var code = string.Empty;
                var passwordHash = string.Empty;
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = sqlScript;
                    command.Parameters.Add(parameter);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            id = reader.GetGuid(0);
                            code = reader.GetString(1);
                            instanceId = reader.GetGuid(2);
                            role = reader.GetGuid(3);
                            passwordHash = reader.GetValueOrDefault<string>(4);
                        }
                    }
                }
                if (id != Guid.Empty
                    && !string.IsNullOrWhiteSpace(passwordHash)
                    && this.ComparePasswords(passwordHash, password))
                {
                    var credentials = new UserCredentials(id, code, role, instanceId);
                    return new ServiceResult<UserCredentials>(credentials);
                }

                return new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
        }

        private static string NewToken(
            UserCredentials credentials,
            TokenSettings settings,
            out Guid tokenId)
        {
            tokenId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(DeliveryTrackerClaims.TokenId, tokenId.ToString()),
                new Claim(DeliveryTrackerClaims.Id, credentials.Id.ToString()),
                new Claim(DeliveryTrackerClaims.Code, credentials.Code),
                new Claim(DeliveryTrackerClaims.Role, credentials.Role.ToString()),
                new Claim(DeliveryTrackerClaims.InstanceId, credentials.InstanceId.ToString()),
            };

            var identity = new ClaimsIdentity(
                claims,
                RefreshTokenAuthType,
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.AddMinutes(settings.Lifetime),
                signingCredentials: new SigningCredentials(
                    settings.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        private async Task<ServiceResult<UserCredentials>> ValidateRefreshTokenAsync(
            string refreshToken,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            var validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = this.refreshTokenSettings.Issuer,
                    ValidAudiences = new[] { this.refreshTokenSettings.Audience },
                    IssuerSigningKeys = new[] { this.refreshTokenSettings.GetSymmetricSecurityKey() },
                    ValidateLifetime = true,
                };
            
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var user = handler.ValidateToken(refreshToken, validationParameters, out var _);
                
                var tokenCredentials = user.Claims.ToUserCredentials();
                var currentCredentials = this.accessor.GetUserCredentials();
                var refreshTokenIdClaim = user.Claims.FirstOrDefault(p => p.Type == DeliveryTrackerClaims.TokenId);

                var success = tokenCredentials.Valid
                              && currentCredentials == tokenCredentials
                              && refreshTokenIdClaim != null
                              && Guid.TryParse(refreshTokenIdClaim.Value, out var refreshTokenId)
                              && await this.HasSessionWithRefreshTokenAsync(
                                  tokenCredentials.Id,
                                  refreshTokenId,
                                  outerConnection);
                
                return success
                    ? new ServiceResult<UserCredentials>(tokenCredentials) 
                    : new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
            catch (SecurityTokenValidationException)
            {
                return new ServiceResult<UserCredentials>(ErrorFactory.AccessDenied());
            }
        }

        private async Task<bool> HasSessionWithRefreshTokenAsync(
            Guid userId,
            Guid tokenId,
            NpgsqlConnectionWrapper outerConnection = null)
        {
            using (var conn = outerConnection?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlHasSessionRefreshToken;
                    command.Parameters.Add(new NpgsqlParameter("user_id", userId));
                    return (await command.ExecuteScalarAsync()).Equals(tokenId);
                }
            }
        }
        
        #endregion
    }
}