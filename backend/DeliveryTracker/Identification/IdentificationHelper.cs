using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Common;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Identification
{
    public static class IdentificationHelper
    {
        public static readonly IReadOnlyList<string> UserColumnList = new List<string>
        {
            "id", 
            "code", 
            "role",
            "instance_id",
            "surname", 
            "name", 
            "patronymic", 
            "phone_number", 
        }.AsReadOnly();
        
        public static string GetUserColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, UserColumnList.Select(p => prefix + p));
        }
        
        
        public static readonly IReadOnlyList<string> SessionColumnList = new List<string>
        {
            "id",
            "user_id",
            "session_token_id", 
            "refresh_token_id", 
            "last_activity"
        }.AsReadOnly();
        
        public static string GetSessionColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, SessionColumnList.Select(p => prefix + p));
        }
        
        
        public static PasswordSettings ReadPasswordSettingsFromConf(IConfiguration configuration)
        {
            return new PasswordSettings(
                SettingsName.Password,
                configuration.GetValue("PasswordSettings:MinLength", 6),
                configuration.GetValue("PasswordSettings:MaxLength", 20),
                configuration.GetValue("PasswordSettings:AtLeastOneUpperCase", false),
                configuration.GetValue("PasswordSettings:AtLeastOneLowerCase", false),
                configuration.GetValue("PasswordSettings:AtLeastOneDigit", false),
                configuration.GetValue("PasswordSettings:Alphabet", string.Empty),
                configuration.GetValue("PasswordSettings:SameCharactersInARow", 20)
            );
        }

        public static TokenSettings ReadTokenSettingsFromConf(IConfiguration configuration)
        {
            return new TokenSettings(
                SettingsName.SessionToken,
                configuration.GetValue<string>("SessionTokenSettings:Key", null) ?? throw new NullReferenceException("specify secret key"),
                configuration.GetValue<string>("SessionTokenSettings:Issuer", null) ?? throw new NullReferenceException("specify issuer"),
                configuration.GetValue<string>("SessionTokenSettings:Audience", null) ?? throw new NullReferenceException("specify audience"),
                configuration.GetValue("SessionTokenSettings:Lifetime", 1),
                configuration.GetValue("SessionTokenSettings:ClockCkew", 1),
                configuration.GetValue("SessionTokenSettings:RequireHttps", true));
        }

        public static TokenSettings ReadRefreshTokenSettingsFromConf(
            IConfiguration configuration)
        {
            return new TokenSettings(
                SettingsName.RefreshToken,
                configuration.GetValue<string>("RefreshTokenSettings:Key", null) ??
                    throw new NullReferenceException("specify secret key"),
                configuration.GetValue<string>("RefreshTokenSettings:Issuer", null) ??
                    throw new NullReferenceException("specify issuer"),
                configuration.GetValue<string>("RefreshTokenSettings:Audience", null) ??
                    throw new NullReferenceException("specify audience"),
                configuration.GetValue("RefreshTokenSettings:Lifetime", 1),
                configuration.GetValue("RefreshTokenSettings:ClockCkew", 1),
                configuration.GetValue("RefreshTokenSettings:RequireHttps", true));
        }
    }
}