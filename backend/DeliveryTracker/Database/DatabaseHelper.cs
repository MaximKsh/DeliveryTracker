using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
    }
}