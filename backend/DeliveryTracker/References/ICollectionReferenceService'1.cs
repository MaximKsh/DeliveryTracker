using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.References
{
    public interface ICollectionReferenceService<T> : ICollectionReferenceService
        where T : ReferenceCollectionBase
    {
        /// <summary>
        /// Создать новую коллекционную запись.
        /// </summary>
        /// <param name="newData"> Новая запись </param>
        /// <param name="oc"> Подключение к PostgreSQL </param>
        /// <returns> Результат создания </returns>
        Task<ServiceResult> CreateAsync(
            T newData, 
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Получить список коллекционных записей по идентификатору родителя.
        /// </summary>
        /// <param name="parentId">Идентификатор родителя</param>
        /// <param name="withDeleted">С учетом удаленных элементов.</param>
        /// <param name="oc"></param>
        /// <returns>Список коллекционных элементов.</returns>
        new Task<ServiceResult<IList<T>>> GetAsync(
            Guid parentId, 
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать элемент коллекции.
        /// </summary>
        /// <param name="newData">Измененные данные в элементе коллекции</param>
        /// <param name="oc">Подключение к PostgreSQL</param>
        /// <returns>Измененные данные.</returns>
        Task<ServiceResult> EditAsync(
            T newData,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Удалить элемент из коллекции.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        new Task<ServiceResult> DeleteAsync(
            Guid id, 
            Guid parentId,
            NpgsqlConnectionWrapper oc = null);
        
    }
}