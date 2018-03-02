using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Instances
{
    public static class InstanceHelper
    {
        public static readonly IReadOnlyList<string> InstanceColumnList = new List<string>
        {
            "id", 
            "name", 
            "creator_id"
        }.AsReadOnly();
        
        public static string GetInstanceColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(InstanceColumnList, prefix);
        
        public static readonly IReadOnlyList<string> InvitationColumnList = new List<string>
        {
            "id", 
            "invitation_code", 
            "creator_id",
            "created",
            "expires", 
            "role",
            "instance_id",
            "surname",
            "name", 
            "patronymic", 
            "phone_number"
        }.AsReadOnly();
        
        public static string GetInvitationColumns(
            string prefix = null) => DatabaseHelper.GetDatabaseColumnsList(InvitationColumnList, prefix);
        
        public static InvitationSettings ReadInvitationSettingsFromConf(IConfiguration configuration)
        {
            return new InvitationSettings(
                    SettingsName.Invitation,
                    configuration.GetValue("InvitationSettings:ExpiresInDays", 3),
                    configuration.GetValue("InvitationSettings:CodeLength", 6),
                    configuration.GetValue("InvitationSettings:Alphabet", "23456789qwertyupasdfghkzxbnmQWERTYUPASDFGHKZXVBNM"))
                ;
        }
    }
}