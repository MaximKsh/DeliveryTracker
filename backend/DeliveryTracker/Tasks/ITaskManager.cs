using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Tasks
{
    public interface ITaskManager
    {
        /// <summary>
        /// Создать новое задание.
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<TaskInfo>> CreateAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать задание.
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<TaskInfo>> EditAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Получить задание.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instanceId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult<TaskInfo>> GetAsync(
            Guid id,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Редактировать информацию о списке товаров.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instanceId"></param>
        /// <param name="taskProducts"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> EditProductsAsync(
            Guid id,
            Guid instanceId,
            IList<TaskProduct> taskProducts,
            NpgsqlConnectionWrapper oc = null);

        /// <summary>
        /// Заполнить информацию о задании списком товаров.
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> FillProductsAsync(
            IList<TaskInfo> taskInfo,
            NpgsqlConnectionWrapper oc = null);
        
        /// <summary>
        /// Удалить задание.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instanceId"></param>
        /// <param name="oc"></param>
        /// <returns></returns>
        Task<ServiceResult> DeleteAsync(
            Guid id,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null);

    }
}