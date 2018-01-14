using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public static class IdentificationHelper
    {
        public static readonly IReadOnlyList<string> UserColumnList = new List<string>
        {
            "id", 
            "code", 
            "role",
            "surname", 
            "name", 
            "patronymic", 
            "phone_number", 
            "instance_id"
        }.AsReadOnly();
        
        public static string GetUserColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, UserColumnList.Select(p => prefix + p));
        }
        
    }
}