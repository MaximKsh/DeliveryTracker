using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Geopositioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Identification
{
    public static class IdentifiactionExtensions
    {
        #region Registration
        
        public static IServiceCollection AddDeliveryTrackerIdentification(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                .AddSingleton<IDeviceManager, DeviceManager>()
                .AddSingleton<IUserCredentialsAccessor, UserCredentialsAccessor>()
                ;

            var tokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(configuration);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = AuthorizationPolicies.DefaultPolicy;
                options.AddPolicy(AuthorizationPolicies.Creator, AuthorizationPolicies.CreatorPolicy);
                options.AddPolicy(AuthorizationPolicies.Manager, AuthorizationPolicies.ManagerPolicy);
                options.AddPolicy(AuthorizationPolicies.CreatorOrManager, AuthorizationPolicies.CreatorOrManagerPolicy);
                options.AddPolicy(AuthorizationPolicies.Performer, AuthorizationPolicies.PerformerPolicy);
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = tokenSettings.RequireHttps;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = tokenSettings.Issuer,

                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = tokenSettings.Audience,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(tokenSettings.ClockCkew),

                        // установка ключа безопасности
                        IssuerSigningKey = tokenSettings.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });


            return services;
        }

        public static ISettingsStorage AddDeliveryTrackerIdentificationSettings(
            this ISettingsStorage storage, 
            IConfiguration configuration)
        {
            var tokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(configuration);
            var refreshTokenSettings = IdentificationHelper.ReadRefreshTokenSettingsFromConf(configuration);
            var passwordSettings = IdentificationHelper.ReadPasswordSettingsFromConf(configuration);
            var sessionSettings = IdentificationHelper.ReadSessionSettingsFromConf(configuration);

            if (sessionSettings.UserOnlineTimeout != -1)
            {
                OnlineChecker.Set(sessionSettings.UserOnlineTimeout);
            }
            
            return storage
                .RegisterSettings(tokenSettings)
                .RegisterSettings(refreshTokenSettings)
                .RegisterSettings(passwordSettings)
                .RegisterSettings(sessionSettings);
        }
        
        #endregion
        
        #region IDataReader
        
        public static User GetUser(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetUser(ref idx);
        }
        
        public static User GetUser(this IDataReader reader, ref int idx)
        {
            var user = new User
            {
                Id = reader.GetGuid(idx++),
                Deleted = reader.GetBoolean(idx++),
                Code = reader.GetString(idx++),
                Role = reader.GetGuid(idx++),
                InstanceId = reader.GetGuid(idx++),
                LastActivity = reader.GetDateTime(idx++),
                Surname = reader.GetValueOrDefault<string>(idx++),
                Name = reader.GetValueOrDefault<string>(idx++),
                Patronymic = reader.GetValueOrDefault<string>(idx++),
                PhoneNumber = reader.GetValueOrDefault<string>(idx++),
            };
            var lon = reader.GetValueOrDefault<double?>(idx++);
            var lat = reader.GetValueOrDefault<double?>(idx++);
            if (lat.HasValue
                && lon.HasValue)
            {
                user.Geoposition = new Geoposition
                {
                    Longitude = lon.Value,
                    Latitude = lat.Value,
                };
            }

            return user;
        }

        public static Session GetSession(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetSession(ref idx);
        }
        
        public static Session GetSession(this IDataReader reader, ref int idx)
        {
            idx++;
            return new Session
            {
                UserId = reader.GetGuid(idx++),
                SessionTokenId = reader.GetValueOrDefault<Guid?>(idx++),
                RefreshTokenId = reader.GetValueOrDefault<Guid?>(idx++),
                LastActivity = reader.GetValueOrDefault<DateTime?>(idx++),
            };
        }
        
        public static Device GetDevice(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetDevice(ref idx);
        }
        
        public static Device GetDevice(this IDataReader reader, ref int idx)
        {
            return new Device
            {
                UserId = reader.GetGuid(idx++),
                Type = reader.GetValueOrDefault<string>(idx++),
                Version = reader.GetValueOrDefault<string>(idx++),
                ApplicationType = reader.GetValueOrDefault<string>(idx++),
                ApplicationVersion = reader.GetValueOrDefault<string>(idx++),
                Language = reader.GetValueOrDefault<string>(idx++),
                FirebaseId = reader.GetValueOrDefault<string>(idx++),
            };
        }
        
        #endregion
        
        public static UserCredentials ToUserCredentials(
            this IEnumerable<Claim> claims)
        {
            var id = Guid.Empty;
            var code = string.Empty;
            var instanceId = Guid.Empty;
            var role = Guid.Empty;
            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case DeliveryTrackerClaims.Id when Guid.TryParse(claim.Value, out var uid):
                        id = uid;
                        break;
                    case DeliveryTrackerClaims.Code:
                        code = claim.Value;
                        break;
                    case DeliveryTrackerClaims.InstanceId when Guid.TryParse(claim.Value, out var iid):
                        instanceId = iid;
                        break;
                    case DeliveryTrackerClaims.Role when Guid.TryParse(claim.Value, out var rid):
                        role = rid;
                        break;
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