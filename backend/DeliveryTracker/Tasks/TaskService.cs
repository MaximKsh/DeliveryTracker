using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Validation;

namespace DeliveryTracker.Tasks
{
    public class TaskService : ITaskService
    {
        #region fields

        private readonly IPostgresConnectionProvider cp;

        private readonly ITaskManager taskManager;

        private readonly ITaskStateTransitionManager stateTransitionManager;

        private readonly IUserCredentialsAccessor accessor;

        private readonly IReferenceFacade referenceFacade;

        private readonly IUserManager userManager;
        
        #endregion
        
        #region constuctor

        public TaskService(
            IPostgresConnectionProvider cp,
            ITaskManager taskManager,
            ITaskStateTransitionManager stateTransitionManager,
            IUserCredentialsAccessor accessor,
            IReferenceFacade referenceFacade,
            IUserManager userManager)
        {
            this.cp = cp;
            this.taskManager = taskManager;
            this.stateTransitionManager = stateTransitionManager;
            this.accessor = accessor;
            this.referenceFacade = referenceFacade;
            this.userManager = userManager;
        }
        
        #endregion
        
        
        #region implementation

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> CreateAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var transaction = connWrapper.BeginTransaction())
                {
                    var createResult = await this.taskManager.CreateAsync(taskInfo, connWrapper);
                    if (!createResult.Success)
                    {
                        transaction.Rollback();
                        return createResult;
                    }

                    var task = createResult.Result;
                    var fillProductsResult = await this.taskManager.FillProductsAsync(new List<TaskInfo> { task }, connWrapper);
                    if (!fillProductsResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<TaskInfo>(fillProductsResult.Errors);
                    }
                    
                    transaction.Commit();
                    return new ServiceResult<TaskInfo>(task);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> EditTaskAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null)
        {
            if (taskInfo.TaskStateId != default)
            {
                return new ServiceResult<TaskInfo>(
                    ErrorFactory.TaskEditError(taskInfo.Id, "Can't change state via edit method."));    
            }

            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var transact = connWrapper.BeginTransaction())
                {
                    var editResult = await this.taskManager.EditAsync(taskInfo, connWrapper);
                    if (!editResult.Success)
                    {
                        transact.Rollback();
                        return editResult;
                    }

                    var editProductsResult = await this.taskManager.EditProductsAsync(
                        taskInfo.Id,
                        taskInfo.InstanceId,
                        taskInfo.TaskProducts,
                        connWrapper);
                    if (!editProductsResult.Success)
                    {
                        transact.Rollback();
                        return editResult;
                    }

                    var newTask = editResult.Result;
                    var fillProductsResult = await this.taskManager.FillProductsAsync(
                        newTask, connWrapper);
                    if (!fillProductsResult.Success)
                    {
                        transact.Rollback();
                        return new ServiceResult<TaskInfo>(fillProductsResult.Errors);
                    }
                    transact.Commit();
                
                    return new ServiceResult<TaskInfo>(newTask);   
                }
            }
                   
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> TransitAsync(
            Guid taskId,
            Guid transitionId,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.GetUserCredentials();
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var canTransit = await this.stateTransitionManager.CanTransit(taskId, credentials.Id, transitionId, connWrapper);
                if (!canTransit.Success)
                {
                    return new ServiceResult<TaskInfo>(canTransit.Errors);
                }

                var newTask = new TaskInfo
                {
                    Id = taskId,
                    InstanceId = credentials.InstanceId,
                    TaskStateId = transitionId,
                };

                var editResult = await this.taskManager.EditAsync(newTask, connWrapper);

                return editResult;
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> GetTaskAsync(
            Guid taskId,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.GetUserCredentials();
            using (var cw = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var result = await this.taskManager.GetAsync(taskId, credentials.InstanceId, cw);
                if (!result.Success)
                {
                    return result;
                }

                var task = result.Result;
                var fillProductsResult = await this.taskManager.FillProductsAsync(
                    task, cw);
                if (!fillProductsResult.Success)
                {
                    return new ServiceResult<TaskInfo>(fillProductsResult.Errors);
                }
            
                return new ServiceResult<TaskInfo>(task);    
            }
        }

        public async Task<ServiceResult<TaskPackage>> PackTaskAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.GetUserCredentials();
            var taskPackage = new TaskPackage();
            using (var cw = oc?.Connect() ?? this.cp.Create().Connect())
            {
                taskPackage.LinkedReferences = new Dictionary<string, DictionaryObject>(8);
                if (taskInfo.TaskProducts?.Count > 0)
                {
                    var result = await this.AddToReferences(
                        taskPackage, 
                        nameof(Product), 
                        taskInfo.TaskProducts.Select(p => p.ProductId).ToArray(), 
                        cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                }
                
                if (taskInfo.WarehouseId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(Warehouse), taskInfo.WarehouseId.Value, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Id.ToString(), result.Result);
                }
                
                if (taskInfo.PaymentTypeId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(PaymentType), taskInfo.PaymentTypeId.Value, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Id.ToString(), result.Result);
                }
                
                if (taskInfo.ClientId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(Client), taskInfo.ClientId.Value, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Id.ToString(), result.Result);
                }

                var userIds = new List<Guid> { taskInfo.AuthorId };
                if (taskInfo.PerformerId.HasValue)
                {
                    userIds.Add(taskInfo.PerformerId.Value);
                }
                var getUsersResult = await this.userManager.GetAsync(userIds, credentials.InstanceId,  cw);
                if (!getUsersResult.Success)
                {
                    return new ServiceResult<TaskPackage>(getUsersResult.Errors);
                }

                taskPackage.LinkedUsers = getUsersResult.Result.ToDictionary(k => k.Id.ToString(), v => v);

                var transitionsResult = await this.stateTransitionManager.GetTransitions(
                    credentials.Role, taskInfo.TaskStateId, cw);
                if (!transitionsResult.Success)
                {
                    return new ServiceResult<TaskPackage>(transitionsResult.Errors);
                }

                taskPackage.LinkedTaskStateTransitions =  transitionsResult.Result;
            }

            return new ServiceResult<TaskPackage>(taskPackage);
        
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskPackage>> PackTasksAsync(
            IList<TaskInfo> taskInfos,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.accessor.GetUserCredentials();
            var taskPackage = new TaskPackage();
            var productIds = new HashSet<Guid>(taskInfos.Count);
            var warehousesIds = new HashSet<Guid>(taskInfos.Count);
            var paymentTypeIds = new HashSet<Guid>(taskInfos.Count);
            var clientsIds = new HashSet<Guid>(taskInfos.Count);
            var userIds = new HashSet<Guid>(2 * taskInfos.Count); // Автор и Исполнитель
            var initialStates = new HashSet<Guid>(taskInfos.Count);
            foreach (var taskInfo in taskInfos)
            {
                if (taskInfo.TaskProducts != null)
                {
                    foreach (var item in taskInfo.TaskProducts.Select(p => p.ProductId))
                    {
                        productIds.Add(item);   
                    }
                }
                if (taskInfo.WarehouseId.HasValue)
                {
                    warehousesIds.Add(taskInfo.WarehouseId.Value);   
                }
                if (taskInfo.PaymentTypeId.HasValue)
                {
                    paymentTypeIds.Add(taskInfo.PaymentTypeId.Value);   
                }
                if (taskInfo.ClientId.HasValue)
                {
                    clientsIds.Add(taskInfo.ClientId.Value);   
                }
                userIds.Add(taskInfo.AuthorId); 
                if (taskInfo.PerformerId.HasValue)
                {
                    userIds.Add(taskInfo.PerformerId.Value);   
                }

                initialStates.Add(taskInfo.TaskStateId);
            }

            using (var cw = oc?.Connect() ?? this.cp.Create().Connect())
            {
                taskPackage.LinkedReferences = new Dictionary<string, DictionaryObject>(
                    productIds.Count + warehousesIds.Count + paymentTypeIds.Count + clientsIds.Count);
                var result = await this.AddToReferences(taskPackage, nameof(Product), productIds, cw);
                if (!result.Success)
                {
                    return new ServiceResult<TaskPackage>(result.Errors);
                }
                result = await this.AddToReferences(taskPackage, nameof(Warehouse), warehousesIds, cw);
                if (!result.Success)
                {
                    return new ServiceResult<TaskPackage>(result.Errors);
                }
                result = await this.AddToReferences(taskPackage, nameof(PaymentType), paymentTypeIds, cw);
                if (!result.Success)
                {
                    return new ServiceResult<TaskPackage>(result.Errors);
                }
                result = await this.AddToReferences(taskPackage, nameof(Client), clientsIds, cw);
                if (!result.Success)
                {
                    return new ServiceResult<TaskPackage>(result.Errors);
                }
                
                var getUsersResult = await this.userManager.GetAsync(userIds, credentials.InstanceId,  cw);
                if (!getUsersResult.Success)
                {
                    return new ServiceResult<TaskPackage>(getUsersResult.Errors);
                }

                taskPackage.LinkedUsers = getUsersResult.Result.ToDictionary(k => k.Id.ToString(), v => v);

                var transitionsResult = await this.stateTransitionManager.GetTransitions(credentials.Role, initialStates, cw);
                if (!transitionsResult.Success)
                {
                    return new ServiceResult<TaskPackage>(transitionsResult.Errors);
                }

                taskPackage.LinkedTaskStateTransitions =  transitionsResult.Result;
            }

            return new ServiceResult<TaskPackage>(taskPackage);
        }
        
        #endregion
        
        #region private

        private async Task<ServiceResult<IList<ReferenceEntityBase>>> AddToReferences(
            TaskPackage taskPackage,
            string type,
            ICollection<Guid> ids,
            NpgsqlConnectionWrapper cw)
        {
            var result = await this.referenceFacade.GetAsync(type, ids, cw);
            if (!result.Success)
            {
                return result;
            }
            foreach (var item in result.Result)
            {
                taskPackage.LinkedReferences.Add(item.Id.ToString(), item);
            }

            return new ServiceResult<IList<ReferenceEntityBase>>();
        }
        
        
        #endregion
    }
    
}