using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    public interface IReferenceService
    {
        /// <summary>
        /// Название справочника.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Описание справочника.
        /// </summary>
        ReferenceDescription ReferenceDescription { get; }
        
        /// <summary>
        /// Создать новую запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> CreateAsync(
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> GetAsync(
            Guid id, 
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить несколько записей из справочника.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<ReferenceEntityBase>>> GetAsync(
            ICollection<Guid> ids, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntityBase>> EditAsync(
            IDictionary<string, object> newData, 
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(
            Guid id, 
            NpgsqlConnectionWrapper oc = null);
    }
}