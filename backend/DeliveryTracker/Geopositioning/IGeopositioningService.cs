using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Geopositioning
{
    public interface IGeopositioningService
    {
        /// <summary>
        /// Обновить координаты текущего пользователя. 
        /// </summary>
        /// <param name="geoposition">
        /// Новые координаты или null.
        /// </param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> UpdateGeopositionAsync(
            Geoposition geoposition,
            NpgsqlConnectionWrapper oc = null);

    }
}