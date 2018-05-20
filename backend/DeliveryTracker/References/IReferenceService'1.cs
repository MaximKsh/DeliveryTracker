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
        where T : ReferenceEntryBase
    {
        /// <summary>
        /// Создать новую запись в справочнике.
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="check"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<T>> CreateAsync(
            T newData, 
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
        new Task<ServiceResult<T>> GetAsync(
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
        new Task<ServiceResult<IList<T>>> GetAsync(
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
        Task<ServiceResult<T>> EditAsync(
            T newData,
            bool check = true,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить запись из справочника.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="check">Проверять разрешение</param>
        /// <param name="oc"></param>
        /// <returns></returns>
        new Task<ServiceResult> DeleteAsync(
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
            T entry,
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
            ICollection<T> entries,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);
    }
}