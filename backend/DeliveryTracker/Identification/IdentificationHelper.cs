using System;
using System.Collections.Generic;
using System.Linq;
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
        
        
        public static PasswordSettings ReadPasswordSettingsFromConf(IConfiguration configuration)
        {
            return new PasswordSettings(
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
                configuration.GetValue<string>("AuthInfo:Key", null) ?? throw new NullReferenceException("specify secret key"),
                configuration.GetValue<string>("AuthInfo:Issuer", null) ?? throw new NullReferenceException("specify issuer"),
                configuration.GetValue<string>("AuthInfo:Audience", null) ?? throw new NullReferenceException("specify audience"),
                configuration.GetValue("AuthInfo:Lifetime", 1),
                configuration.GetValue("AuthInfo:ClockCkew", 1),
                configuration.GetValue("AuthInfo:RequireHttps", true));
        }
    }
}