using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    /// <summary>
    /// Сервис для взаимодействия с справочником.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReferenceService<T> : IReferenceService
        where T : ReferenceEntityBase
    {
        /// <summary>
        /// Создать новую запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<T>> CreateAsync(T newData, NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        new Task<ServiceResult<T>> GetAsync(Guid id, NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить несколько записей из справочника.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        new Task<ServiceResult<IList<T>>> GetAsync(
            ICollection<Guid> ids, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<T>> EditAsync(T newData, NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        new Task<ServiceResult> DeleteAsync(Guid id, NpgsqlConnectionWrapper oc = null);
    }
}