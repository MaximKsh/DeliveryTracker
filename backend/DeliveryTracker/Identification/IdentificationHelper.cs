using System;
using System.Collections.Generic;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Identification
{
    public static class IdentificationHelper
    {
        public static readonly IReadOnlyList<string> UserColumnList = new List<string>
        {
            "id", 
            "deleted",
            "code", 
            "role",
            "instance_id",
            "last_activity",
            "surname", 
            "name", 
            "patronymic", 
            "phone_number",
            "ST_X(geoposition::geometry)",
            "ST_Y(geoposition::geometry)",
        }.AsReadOnly();
        
        public static string GetUserColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(UserColumnList, prefix);
        
        public static readonly IReadOnlyList<string> SessionColumnList = new List<string>
        {
            "id",
            "user_id",
            "session_token_id", 
            "refresh_token_id", 
            "last_activity"
        }.AsReadOnly();
        
        public static string GetSessionColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(SessionColumnList, prefix);
        
        public static readonly IReadOnlyList<string> DeviceColumnList = new List<string>
        {
            "user_id",
            "device_type",
            "device_version",
            "application_type",
            "application_version",
            "language",
            "firebase_id",
        }.AsReadOnly();
        
        public static string GetDeviceColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(DeviceColumnList, prefix);
        
        
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
        
        public static SessionSettings ReadSessionSettingsFromConf(IConfiguration configuration)
        {
            return new SessionSettings(
                SettingsName.Session,
                configuration.GetValue("SessionSettings:UserOnlineTimeout", -1),
                configuration.GetValue("SessionSettings:SessionInactiveTimeout", -1));
        }
    }
}