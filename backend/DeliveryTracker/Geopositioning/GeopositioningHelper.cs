using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryTracker.Geopositioning
{
    public class GeopositioningHelper
    {
        public static readonly IReadOnlyList<string> GeopositionColumnList = new List<string>
        {
            "latitude", 
            "longitude", 
        }.AsReadOnly();
        
        public static string GetGeopositionColumns(string prefix = null)
        {
            prefix = prefix ?? string.Empty;
            return string.Join("," + Environment.NewLine, GeopositionColumnList.Select(p => prefix + p));
        }
    }
}