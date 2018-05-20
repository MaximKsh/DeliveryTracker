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
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntryBase>> CreateAsync(
            IDictionary<string, object> newData,
            bool check = true,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withDeleted"></param>
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntryBase>> GetAsync(
            Guid id, 
            bool withDeleted = false,
            bool check = true,
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить несколько записей из справочника.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="withDeleted"></param>
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<ReferenceEntryBase>>> GetAsync(
            ICollection<Guid> ids, 
            bool withDeleted = false,
            bool check = true,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferenceEntryBase>> EditAsync(
            IDictionary<string, object> newData, 
            bool check = true,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(
            Guid id, 
            bool check = true,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Запаковать запись из справочника.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="withDeleted"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<ReferencePackage>> PackAsync(
            ReferenceEntryBase entry,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Запаковать несколько записей из справочника.
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="withDeleted"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<IList<ReferencePackage>>> PackAsync(
            ICollection<ReferenceEntryBase> entries,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);
    }
}