using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Tasks.TransitionObservers;
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

        private readonly ITaskObserverExecutor observerExecutor;
        
        #endregion
        
        #region constuctor

        public TaskService(
            IPostgresConnectionProvider cp,
            ITaskManager taskManager,
            ITaskStateTransitionManager stateTransitionManager,
            IUserCredentialsAccessor accessor,
            IReferenceFacade referenceFacade,
            IUserManager userManager,
            ITaskObserverExecutor observerExecutor)
        {
            this.cp = cp;
            this.taskManager = taskManager;
            this.stateTransitionManager = stateTransitionManager;
            this.accessor = accessor;
            this.referenceFacade = referenceFacade;
            this.userManager = userManager;
            this.observerExecutor = observerExecutor;
        }
        
        #endregion
        
        
        #region implementation

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> CreateAsync(
            TaskPackage taskPackage,
            NpgsqlConnectionWrapper oc = null)
        {
            
            var credentials = this.accessor.GetUserCredentials();
            if (credentials.Role != DefaultRoles.CreatorRole
                && credentials.Role != DefaultRoles.ManagerRole)
            {
                return new ServiceResult<TaskInfo>(ErrorFactory.AccessDenied());
            }

            if (taskPackage.TaskInfo?.Count > 0 != true)
            {
                return new ServiceResult<TaskInfo>(ErrorFactory.TaskCreationError());
            }

            var taskInfo = taskPackage.TaskInfo[0];
            var taskProducts = taskPackage.TaskProducts;
            
            taskInfo.Id = Guid.NewGuid();
            taskInfo.AuthorId = credentials.Id;
            taskInfo.InstanceId = credentials.InstanceId;
            taskInfo.SetState(DefaultTaskStates.Preparing);
            
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
                    
                    var editProductsResult = await this.taskManager.EditProductsAsync(
                        taskInfo.Id,
                        taskInfo.InstanceId,
                        taskProducts,
                        connWrapper);
                    if (!editProductsResult.Success)
                    {
                        transaction.Rollback();
                        return new ServiceResult<TaskInfo>(editProductsResult.Errors);
                    }

                    var transitionsResult = await this.stateTransitionManager.GetTransitions(
                        credentials.Role, task.TaskStateId, connWrapper);
                    if (!transitionsResult.Success)
                    {
                        return new ServiceResult<TaskInfo>(transitionsResult.Errors);
                    }

                    task.TaskStateTransitions = transitionsResult.Result;
                    
                    var creds = this.accessor.GetUserCredentials();
                    var ctx = new TaskObserverContext(null, taskInfo, creds, null, connWrapper);
                    await this.observerExecutor.ExecuteNew(ctx);
                    if (ctx.Cancel)
                    {
                        transaction.Rollback();
                        return new ServiceResult<TaskInfo>(ctx.Errors);
                    }
                    
                    transaction.Commit();
                    return new ServiceResult<TaskInfo>(task);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<TaskInfo>> EditTaskAsync(
            TaskPackage taskPackage,
            NpgsqlConnectionWrapper oc = null)
        {
            if (taskPackage.TaskInfo?.Count > 0 != true)
            {
                return new ServiceResult<TaskInfo>(ErrorFactory.TaskCreationError());
            }

            var creds = this.accessor.GetUserCredentials();
            var taskInfo = taskPackage.TaskInfo[0];
            var taskProducts = taskPackage.TaskProducts;
            
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

                    var task = editResult.Result;

                    var editProductsResult = await this.taskManager.EditProductsAsync(
                        taskInfo.Id,
                        creds.InstanceId,
                        taskProducts,
                        connWrapper);
                    if (!editProductsResult.Success)
                    {
                        transact.Rollback();
                        return new ServiceResult<TaskInfo>(editProductsResult.Errors);
                    }

                    var transitionsResult = await this.stateTransitionManager.GetTransitions(
                        creds.Role, task.TaskStateId, connWrapper);
                    if (!transitionsResult.Success)
                    {
                        return new ServiceResult<TaskInfo>(transitionsResult.Errors);
                    }

                    task.TaskStateTransitions = transitionsResult.Result;

                    var ctx = new TaskObserverContext(taskInfo, task, creds, null, connWrapper);
                    await this.observerExecutor.ExecuteEdit(ctx);
                    if (ctx.Cancel)
                    {
                        transact.Rollback();
                        return new ServiceResult<TaskInfo>(ctx.Errors);
                    }
                    
                    transact.Commit();
                
                    return new ServiceResult<TaskInfo>(task);   
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
            using(var transact = connWrapper.BeginTransaction())
            {
                var canTransit = await this.stateTransitionManager.CanTransit(taskId, credentials.Id, transitionId, connWrapper);
                if (!canTransit.Success)
                {
                    return new ServiceResult<TaskInfo>(canTransit.Errors);
                }

                var getTransitionResult = await this.stateTransitionManager.GetTransition(transitionId, connWrapper);

                var taskResult = await this.taskManager.GetAsync(
                    taskId, 
                    credentials.InstanceId,
                    connWrapper);
                if (!taskResult.Success)
                {
                    return taskResult;
                }

                var newTask = taskResult.Result;
                newTask.TaskStateId = getTransitionResult.Result.FinalState;
                
                var editResult = await this.taskManager.EditAsync(newTask, connWrapper);
                
                var creds = this.accessor.GetUserCredentials();
                var ctx = new TaskObserverContext(null, editResult.Result, creds, getTransitionResult.Result, connWrapper);
                await this.observerExecutor.ExecuteTransition(ctx);
                if (ctx.Cancel)
                {
                    transact.Rollback();
                    return new ServiceResult<TaskInfo>(ctx.Errors);
                }

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
                
                var transitionsResult = await this.stateTransitionManager.GetTransitions(
                    credentials.Role, task.TaskStateId, cw);
                if (!transitionsResult.Success)
                {
                    return new ServiceResult<TaskInfo>(transitionsResult.Errors);
                }

                task.TaskStateTransitions = transitionsResult.Result;
                
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
                var taskProductsResult = await this.taskManager.GetProductsAsync(taskInfo.Id, credentials.InstanceId, cw);
                if (!taskProductsResult.Success)
                {
                    return new ServiceResult<TaskPackage>(taskProductsResult.Errors);
                }

                taskPackage.TaskProducts = taskProductsResult.Result;
                
                taskPackage.LinkedReferences = new Dictionary<string, DictionaryObject>(8);
                if (taskPackage.TaskProducts?.Count > 0)
                {
                    var result = await this.AddToReferences(
                        taskPackage, 
                        nameof(Product), 
                        taskPackage.TaskProducts.Select(p => p.ProductId).ToArray(), 
                        cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                }
                
                if (taskInfo.WarehouseId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(Warehouse), taskInfo.WarehouseId.Value, true, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Entry.Id.ToString(), result.Result);
                }
                
                if (taskInfo.PaymentTypeId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(PaymentType), taskInfo.PaymentTypeId.Value, true, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Entry.Id.ToString(), result.Result);
                }
                
                if (taskInfo.ClientId.HasValue)
                {
                    var result = await this.referenceFacade.GetAsync(nameof(Client), taskInfo.ClientId.Value, true, cw);
                    if (!result.Success)
                    {
                        return new ServiceResult<TaskPackage>(result.Errors);
                    }
                    taskPackage.LinkedReferences.Add(result.Result.Entry.Id.ToString(), result.Result);
                }

                var userIds = new List<Guid> { taskInfo.AuthorId };
                if (taskInfo.PerformerId.HasValue)
                {
                    userIds.Add(taskInfo.PerformerId.Value);
                }
                var getUsersResult = await this.userManager.GetAsync(userIds, credentials.InstanceId, true, oc: cw);
                if (!getUsersResult.Success)
                {
                    return new ServiceResult<TaskPackage>(getUsersResult.Errors);
                }

                taskPackage.LinkedUsers = getUsersResult.Result.ToDictionary(k => k.Id.ToString(), v => v);

            }

            taskPackage.TaskInfo = new List<TaskInfo> { taskInfo };
            
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
                var taskProductsResult = await this.taskManager.GetProductsAsync(
                    taskInfos.Select(p => p.Id), credentials.InstanceId, cw);
                if (!taskProductsResult.Success)
                {
                    return new ServiceResult<TaskPackage>(taskProductsResult.Errors);
                }

                taskPackage.TaskProducts = taskProductsResult.Result;
                if (taskPackage.TaskProducts.Count > 0)
                {
                    foreach (var item in taskPackage.TaskProducts.Select(p => p.ProductId))
                    {
                        productIds.Add(item);   
                    }
                }
                
                
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
                
                var getUsersResult = await this.userManager.GetAsync(userIds, credentials.InstanceId, true, oc: cw);
                if (!getUsersResult.Success)
                {
                    return new ServiceResult<TaskPackage>(getUsersResult.Errors);
                }

                taskPackage.LinkedUsers = getUsersResult.Result.ToDictionary(k => k.Id.ToString(), v => v);
            }
            taskPackage.TaskInfo = new List<TaskInfo>( taskInfos );

            return new ServiceResult<TaskPackage>(taskPackage);
        }
        
        #endregion
        
        #region private

        private async Task<ServiceResult> AddToReferences(
            TaskPackage taskPackage,
            string type,
            ICollection<Guid> ids,
            NpgsqlConnectionWrapper cw)
        {
            if (ids.Count == 0)
            {
                return ServiceResult.Successful;
            }
            var result = await this.referenceFacade.GetAsync(type, ids, true, cw);
            if (!result.Success)
            {
                return result;
            }
            foreach (var item in result.Result)
            {
                taskPackage.LinkedReferences.Add(item.Entry.Id.ToString(), item);
            }

            return ServiceResult.Successful;
        }
        
        #endregion
    }
    
}