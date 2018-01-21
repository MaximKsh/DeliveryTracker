using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public interface IView
    {
        
        /// <summary>
        /// Название представления.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Получить результат представления.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="userCredentials"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ServiceResult<object[]>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters);


        /// <summary>
        /// Получить количество записей в представлении.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="userCredentials"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IImmutableDictionary<string, string[]> parameters);
    }
}