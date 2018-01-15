using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public static string GetInstanceColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, InstanceColumnList.Select(p => prefix + p));
        }
        
        public static readonly IReadOnlyList<string> InvitationColumnList = new List<string>
        {
            "id", 
            "invitation_code", 
            "created",
            "expires", 
            "role",
            "instance_id",
            "surname",
            "name", 
            "patronymic", 
            "phone_number"
        }.AsReadOnly();
        
        public static string GetInvitationColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, InvitationColumnList.Select(p => prefix + p));
        }
    }
}