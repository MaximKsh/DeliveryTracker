using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Tasks
{
    public sealed class TaskManager : ITaskManager
    {
        #region sql
        
        private static readonly string SqlCreate = $@"
insert into ""tasks"" ({TaskHelper.GetTasksColumns()})
values ({TaskHelper.GetTasksColumns("@")})
returning {TaskHelper.GetTasksColumns()}
;
";
        
        private static readonly string SqlEdit = @"
update ""tasks""
set
{0}
where id = @id and instance_id = @instance_id
returning " + TaskHelper.GetTasksColumns() + ";";
        
        private static readonly string SqlGetTask = $@"
select {TaskHelper.GetTasksColumns()}
from ""tasks""
where ""id"" = @id and instance_id = @instance_id
;
";

        private static readonly string SqlEditTaskProducts = $@"
insert into task_products(id, instance_id, task_id, {TaskHelper.GetTaskProductsColumns()})
    select
        t.id,
        @instance_id,
        @task_id,
        t.pid,
        t.q
    from
        unnest(
            @ids,
            @product_ids,
            @quantities
        ) with ordinality t(id, pid, q)
on conflict (task_id, product_id) do update 
set quantity = EXCLUDED.quantity
returning id, quantity
;";

        private const string SqlDeleteTaskProducts = @"
delete from task_products
where id = ANY(@ids)
;
";
        private static readonly string SqlGetOneTaskProducts = $@"
select {TaskHelper.GetTaskProductsColumns()}
from task_products
where task_id = @id and instance_id = @instance_id
;";
        

        private static readonly string SqlGetTaskProducts = $@"
select {TaskHelper.GetTaskProductsColumns()}, task_id
from task_products
where task_id = ANY(@ids) and instance_id = @instance_id
;";
        
        
        private static readonly string SqlDeleteTask = $@"
delete from ""tasks""
where ""id"" = @id and instance_id = @instance_id
;
;
";
        
        #endregion
        
        #region fields

        private readonly IPostgresConnectionProvider cp;

        #endregion

        #region constructor

        public TaskManager(
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }        

        #endregion
        
        #region implementation
        
        public async Task<ServiceResult<TaskInfo>> CreateAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null)
        {
            if (!DefaultTaskStates.AllTaskStates.ContainsKey(taskInfo.TaskStateId))
            {
                return new ServiceResult<TaskInfo>(ErrorFactory.IncorrectTaskState(taskInfo.TaskStateId));
            }
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(taskInfo), taskInfo)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.Id)}", taskInfo.Id)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.InstanceId)}", taskInfo.InstanceId)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.AuthorId)}", taskInfo.AuthorId)
                .AddNotNullOrWhitespaceRule($"{nameof(taskInfo)}.{nameof(taskInfo.TaskNumber)}", taskInfo.TaskNumber)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<TaskInfo>(validationResult.Error);
            }

            return await this.CreateAsyncInternal(taskInfo, oc);
        }

        public async Task<ServiceResult<TaskInfo>> EditAsync(
            TaskInfo taskInfo,
            NpgsqlConnectionWrapper oc = null)
        {
            if (taskInfo.TaskStateId != default
                && !DefaultTaskStates.AllTaskStates.ContainsKey(taskInfo.TaskStateId))
            {
                return new ServiceResult<TaskInfo>(ErrorFactory.IncorrectTaskState(taskInfo.TaskStateId));
            }
            
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(taskInfo), taskInfo)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.Id)}", taskInfo.Id)
                .AddNotEmptyGuidRule($"{nameof(taskInfo)}.{nameof(taskInfo.InstanceId)}", taskInfo.InstanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<TaskInfo>(validationResult.Error);
            }

            return await this.EditAsyncInternal(taskInfo, oc);
        }

        public async Task<ServiceResult<TaskInfo>> GetAsync(
            Guid id,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule("id", id)
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<TaskInfo>(validationResult.Error);
            }
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetTask;
                    command.Parameters.Add(new NpgsqlParameter("id", id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<TaskInfo>(reader.GetTaskInfo());
                        }
                    }
                }
            }
            return new ServiceResult<TaskInfo>(ErrorFactory.TaskNotFound(id));
        }

        public async Task<ServiceResult> EditProductsAsync(
            Guid id,
            Guid instanceId,
            IList<TaskProduct> taskProducts,
            NpgsqlConnectionWrapper oc = null)
        {
            if (taskProducts == null
                || taskProducts.Count == 0)
            {
                return new ServiceResult();
            }
            
            var taskProductsDistinct = new HashSet<TaskProduct>((int)(1.5 * taskProducts.Count));
            foreach (var product in taskProducts)
            {
                if (taskProductsDistinct.TryGetValue(product, out var actualProduct))
                {
                    actualProduct.Quantity += product.Quantity;
                }
                else
                {
                    taskProductsDistinct.Add(product);
                }
            }

            var ids = new Guid[taskProductsDistinct.Count];
            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = Guid.NewGuid();
            }
            
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                var toDeleteList = new List<Guid>(taskProducts.Count);
                using (var transaction = connWrapper.BeginTransaction())
                {
                    using (var command = connWrapper.CreateCommand())
                    {
                        command.CommandText = SqlEditTaskProducts;
                        command.Parameters.Add(new NpgsqlParameter("task_id", id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                        command.Parameters.Add(new NpgsqlParameter("ids", ids).WithArrayType(NpgsqlDbType.Uuid));
                        command.Parameters.Add(
                            new NpgsqlParameter("product_ids", taskProductsDistinct.Select(p => p.ProductId).ToArray())
                                .WithArrayType(NpgsqlDbType.Uuid));
                        command.Parameters.Add(
                            new NpgsqlParameter("quantities", taskProductsDistinct.Select(p => p.Quantity).ToArray())
                                .WithArrayType(NpgsqlDbType.Integer));

                        try
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (reader.Read())
                                {
                                    if (reader.GetInt32(1) <= 0)
                                    {
                                        toDeleteList.Add(reader.GetGuid(0));   
                                    }
                                }
                            }
                        }
                        catch (NpgsqlException)
                        {
                            transaction.Rollback();
                            return new ServiceResult(ErrorFactory.TaskEditError(id));
                        }
                    }

                    if (toDeleteList.Count != 0)
                    {
                        using (var command = connWrapper.CreateCommand())
                        {
                            command.CommandText = SqlDeleteTaskProducts;
                            command.Parameters.Add(new NpgsqlParameter("ids", toDeleteList).WithArrayType(NpgsqlDbType.Uuid));
                            try
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                            catch (NpgsqlException)
                            {
                                transaction.Rollback();
                                return new ServiceResult(ErrorFactory.TaskEditError(id));
                            }
                        }    
                    }
                    transaction.Commit();
                }
            }
            return new ServiceResult();
        }

        public async Task<ServiceResult<IList<TaskProduct>>> GetProductsAsync(
            IEnumerable<Guid> taskIds,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null)
        {
            var idArray = taskIds.ToArray();
            if (idArray.Length == 0)
            {
                return new ServiceResult<IList<TaskProduct>>();
            }

            var taskProducts = new List<TaskProduct>();
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetTaskProducts;
                    command.Parameters.Add(new NpgsqlParameter("ids", idArray).WithArrayType(NpgsqlDbType.Uuid));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var idx = 0;
                            var taskProduct = reader.GetTaskProduct(ref idx);
                            taskProducts.Add(taskProduct);        
                        }
                    }
                }
            }

            return new ServiceResult<IList<TaskProduct>>(taskProducts);
        }
        
        public async Task<ServiceResult<IList<TaskProduct>>> GetProductsAsync(
            Guid taskId,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null)
        {
            var taskProducts = new List<TaskProduct>();
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetOneTaskProducts;
                    command.Parameters.Add(new NpgsqlParameter("id", taskId));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            taskProducts.Add(reader.GetTaskProduct());
                        }
                    }
                }
            }

            return new ServiceResult<IList<TaskProduct>>(taskProducts);
        }

        public async Task<ServiceResult> DeleteAsync(
            Guid id,
            Guid instanceId,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule("id", id)
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult(validationResult.Error);
            }
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlDeleteTask;
                    command.Parameters.Add(new NpgsqlParameter("id", id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected == 1 
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.UserNotFound(id)); 
                }
            }
        }
        
        #endregion

        #region private

        private async Task<ServiceResult<TaskInfo>> CreateAsyncInternal(TaskInfo taskInfo, NpgsqlConnectionWrapper oc = null)
        {
            TaskInfo newTask = null;
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", taskInfo.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", taskInfo.InstanceId));
                    command.Parameters.Add(new NpgsqlParameter("state_id", taskInfo.TaskStateId));
                    command.Parameters.Add(new NpgsqlParameter("author_id", taskInfo.AuthorId));
                    command.Parameters.Add(new NpgsqlParameter("performer_id", taskInfo.PerformerId).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("task_number", taskInfo.TaskNumber));
                    command.Parameters.Add(new NpgsqlParameter("created", DateTime.UtcNow));
                    command.Parameters.Add(new NpgsqlParameter("state_changed_last_time", DateTime.UtcNow));
                    command.Parameters.Add(new NpgsqlParameter("receipt", taskInfo.Receipt).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("receipt_actual", taskInfo.ReceiptActual).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("delivery_from", taskInfo.DeliveryFrom).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("delivery_to", taskInfo.DeliveryTo).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("delivery_actual", taskInfo.DeliveryActual).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("comment", taskInfo.Comment).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("warehouse_id", taskInfo.WarehouseId).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("client_id", taskInfo.ClientId).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("client_address_id", taskInfo.ClientAddressId).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("payment_type_id", taskInfo.PaymentTypeId).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("cost", taskInfo.Cost).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("delivery_cost", taskInfo.DeliveryCost).CanBeNull());

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                newTask = reader.GetTaskInfo();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<TaskInfo>(ErrorFactory.TaskCreationError());
                    }
                }
            }
            
            return new ServiceResult<TaskInfo>(newTask);
        }
        
        private async Task<ServiceResult<TaskInfo>> EditAsyncInternal(TaskInfo taskInfo, NpgsqlConnectionWrapper oc = null)
        {
            TaskInfo updatedTaskInfo = null;
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    var builder = new StringBuilder();
                    var parametersCounter = 0; 
                    
                    command.Parameters.Add(new NpgsqlParameter("id", taskInfo.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", taskInfo.InstanceId));
                    if (taskInfo.TaskStateId != default)
                    {
                        parametersCounter += command.AppendIfNotDefault(builder, "state_id", taskInfo.TaskStateId);
                        parametersCounter += command.AppendIfNotDefault(builder, "state_changed_last_time", DateTime.UtcNow);
                    }
                    parametersCounter += command.AppendIfNotDefault(builder, "performer_id", taskInfo.PerformerId);
                    parametersCounter += command.AppendIfNotDefault(builder, "task_number", taskInfo.TaskNumber);
                    parametersCounter += command.AppendIfNotDefault(builder, "receipt", taskInfo.Receipt);
                    parametersCounter += command.AppendIfNotDefault(builder, "receipt_actual", taskInfo.ReceiptActual);
                    parametersCounter += command.AppendIfNotDefault(builder, "delivery_from", taskInfo.DeliveryFrom);
                    parametersCounter += command.AppendIfNotDefault(builder, "delivery_to", taskInfo.DeliveryTo);
                    parametersCounter += command.AppendIfNotDefault(builder, "delivery_actual", taskInfo.DeliveryActual);
                    parametersCounter += command.AppendIfNotDefault(builder, "comment", taskInfo.Comment);
                    parametersCounter += command.AppendIfNotDefault(builder, "warehouse_id", taskInfo.WarehouseId);
                    parametersCounter += command.AppendIfNotDefault(builder, "client_id", taskInfo.ClientAddressId);
                    parametersCounter += command.AppendIfNotDefault(builder, "client_address_id", taskInfo.ClientAddressId);
                    parametersCounter += command.AppendIfNotDefault(builder, "payment_type_id", taskInfo.PaymentTypeId);
                    parametersCounter += command.AppendIfNotDefault(builder, "cost", taskInfo.Cost);
                    parametersCounter += command.AppendIfNotDefault(builder, "delivery_cost", taskInfo.DeliveryCost);
                    
                    command.CommandText = parametersCounter != 0
                        ? string.Format(SqlEdit, builder.ToString())
                        : SqlGetTask;
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                updatedTaskInfo = reader.GetTaskInfo();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<TaskInfo>(ErrorFactory.TaskEditError(taskInfo.Id));
                    }
                }
            }

            return updatedTaskInfo != null
                ? new ServiceResult<TaskInfo>(updatedTaskInfo)
                : new ServiceResult<TaskInfo>(ErrorFactory.TaskNotFound(taskInfo.Id));
        }

        #endregion
    }
}