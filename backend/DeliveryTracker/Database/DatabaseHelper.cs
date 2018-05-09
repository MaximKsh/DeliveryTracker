using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DeliveryTracker.Common;
using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Database
{
    public static class DatabaseHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]   
        public static string GetDatabaseColumnsList(IEnumerable<string> columns, string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, columns.Select(p => prefix + p));
        }
        
        public static DatabaseSettings ReadDatabaseSettingsFromConf(IConfiguration configuration)
        {
            return new DatabaseSettings(
                SettingsName.Database,
                configuration.GetConnectionString("DefaultConnection"));
        }
    }
}