using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;

namespace DeliveryTracker.Tasks
{
    public interface ITaskService
    {
        Task<ServiceResult<TaskInfo>> CreateAsync(
            TaskPackage taskInfo,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<TaskInfo>> EditTaskAsync(
            TaskPackage taskInfo,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<TaskInfo>> TransitAsync(
            Guid taskId,
            Guid transitionId,
            NpgsqlConnectionWrapper oc = null);

        Task<ServiceResult<TaskInfo>> GetTaskAsync(
            Guid taskId,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<TaskPackage>> PackTaskAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null);
        
        Task<ServiceResult<TaskPackage>> PackTasksAsync(
            IList<TaskInfo> taskInfos,
            NpgsqlConnectionWrapper oc = null);
    }
}