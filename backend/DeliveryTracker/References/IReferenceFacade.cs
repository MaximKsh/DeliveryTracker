using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    public interface IReferenceFacade
    {
        
        /// <summary>
        /// Получить список доступных справочников.
        /// </summary>
        /// <returns></returns>
        string[] GetReferencesList();
        
        /// <summary>
        /// Создать новую запись в справочнике
        /// </summary>
        /// <param name="type">Тип справочника</param>
        /// <param name="value">Создаваемый объект</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> CreateAsync(
            string type,
            IDictionary<string, object> value,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить запись из справочника.
        /// </summary>
        /// <param name="type">Тип справочника</param>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> GetAsync(
            string type,
            Guid id, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать запись в справочнике.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> EditAsync(
            string type, 
            IDictionary<string, object> newData, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить запись из справочника.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(
            string type,
            Guid id, 
            NpgsqlConnectionWrapper oc = null);
    }
}