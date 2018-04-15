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
        IReadOnlyDictionary<string, ReferenceDescription> GetReferencesList();
        
        /// <summary>
        /// Создать новую запись в справочнике
        /// </summary>
        /// <param name="type">Тип справочника</param>
        /// <param name="value">Создаваемый объект</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferencePackage>> CreateAsync(
            string type,
            IDictionary<string, object> value,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить запись из справочника.
        /// </summary>
        /// <param name="type">Тип справочника</param>
        /// <param name="id"></param>
        /// <param name="withDeleted"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferencePackage>> GetAsync(
            string type,
            Guid id, 
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить несколько записей из справочника.
        /// </summary>
        /// <param name="type">Тип справочника</param>
        /// <param name="ids"></param>
        /// <param name="withDeleted"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<ReferencePackage>>> GetAsync(
            string type,
            ICollection<Guid> ids,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Редактировать запись в справочнике.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newData"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferencePackage>> EditAsync(
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