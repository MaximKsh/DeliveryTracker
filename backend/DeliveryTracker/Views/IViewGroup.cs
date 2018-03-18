using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Views
{
    public interface IViewGroup
    {
        /// <summary>
        /// Название группы представлений.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Получить список названий представлений в группе.
        /// </summary>
        /// <returns></returns>
        ServiceResult<IList<string>> GetViewsList();

        /// <summary>
        /// Получить дайджест (имя-количество записей) по каждому представлению.
        /// </summary>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<Dictionary<string, ViewDigest>>> GetDigestAsync(
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Выполнить представление и получить результат в качестве типизированного массива.
        /// Более долгая операция, нежели получения массива object-ов.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="parameters"></param>
        /// <param name="oc"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<ServiceResult<IList<T>>> ExecuteViewAsync<T>(
            string viewName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Выполнить представление.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="parameters"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<IDictionaryObject>>> ExecuteViewAsync(
            string viewName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
            NpgsqlConnectionWrapper oc = null);

    }
}