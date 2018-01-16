using System;
using System.Data;
using DeliveryTracker.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryTracker.Identification
{
    public static class IdentifiactionExtensions
    {
        public static IServiceCollection AddDeliveryTrackerIdentification(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services
                .AddSingleton<IUserManager, UserManager>()
                .AddSingleton<ISecurityManager, SecurityManager>()
                
                .AddSingleton<IUserCredentialsAccessor, UserCredentialsAccessor>()
                ;

            var tokenSettings = IdentificationHelper.ReadTokenSettingsFromConf(configuration);
            services.AddSingleton(tokenSettings);

            var passwordSettings = IdentificationHelper.ReadPasswordSettingsFromConf(configuration);
            services.AddSingleton(passwordSettings);

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

        public static User GetUser(this IDataReader reader)
        {
            var idx = 0;
            return reader.GetUser(ref idx);
        }
        
        public static User GetUser(this IDataReader reader, ref int idx)
        {
            return new User
            {
                Id = reader.GetGuid(idx++),
                Code = reader.GetString(idx++),
                Role = reader.GetString(idx++),
                Surname = reader.GetString(idx++),
                Name = reader.GetString(idx++),
                Patronymic = reader.GetValueOrDefault<string>(idx++),
                PhoneNumber = reader.GetValueOrDefault<string>(idx++),
                InstanceId = reader.GetGuid(idx++),
            };
        }
    }
}