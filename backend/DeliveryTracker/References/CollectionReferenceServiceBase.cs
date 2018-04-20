using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.References
{
    public abstract class CollectionReferenceServiceBase<T> : ReferenceServiceBase<T>, ICollectionReferenceService<T> 
        where T : ReferenceCollectionBase, new()
    {
        #region fields

        protected readonly IPostgresConnectionProvider Cp;

        protected readonly IUserCredentialsAccessor Accessor;

        #endregion

        #region constructor

        protected CollectionReferenceServiceBase(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor)
        {
            this.Cp = cp;
            this.Accessor = accessor;
        }

        #endregion
        
        #region implementation

        public string Name { get; } = typeof(T).Name;

        protected sealed override T Read(
            IDataReader reader,
            ReferenceServiceExecutionContext ctx) => null;

        protected sealed override bool CanGetList(
            ICollection<Guid> ids,
            UserCredentials credentials) => false;

        protected sealed override ExecutionParameters SetCommandGetList(
            NpgsqlCommand command,
            ICollection<Guid> ids,
            bool withDeleted,
            UserCredentials credentials) => null;
        
        
        /// <inheritdoc />
        public async Task<ServiceResult> CreateAsync(
            T newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.Name, newData)
                .AddNotEmptyGuidRule(nameof(newData.InstanceId), newData.InstanceId)
                .AddNotEmptyGuidRule(nameof(newData.ParentId), newData.ParentId)
                .AddRule($"{nameof(newData.Action)} != {(int)ReferenceAction.Create} ({ReferenceAction.Create.ToString()})", 
                    newData,
                    p => p.Action == ReferenceAction.Create)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult(validationResult.Error);
            }

            var credentials = this.Accessor.GetUserCredentials();
            if (!this.CanCreate(newData, credentials))
            {
                return new ServiceResult(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var transact = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        var id = newData.Id == default 
                            ? Guid.NewGuid()
                            : newData.Id;
                        command.Parameters.Add(new NpgsqlParameter("id", id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                        command.Parameters.Add(new NpgsqlParameter("parent_id", newData.ParentId));
                        command.Parameters.Add(new NpgsqlParameter("deleted", false));

                        this.SetCommandCreate(command, newData, id, credentials);
                        try
                        {
                            var cnt = await command.ExecuteNonQueryAsync();
                            if (cnt != 1)
                            {
                                transact.Rollback();
                                return new ServiceResult<T>(ErrorFactory.ReferenceCreationError(this.Name));
                            }
                        }
                        catch (NpgsqlException)
                        {
                            transact.Rollback();
                            return new ServiceResult<T>(ErrorFactory.ReferenceCreationError(this.Name));
                        }
                    }
                    transact.Commit();
                }
            }
            return new ServiceResult();
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IList<T>>> GetAsync(
            Guid parentId,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.GetUserCredentials();
            if (!this.CanGet(parentId, credentials))
            {
                return new ServiceResult<IList<T>>(ErrorFactory.AccessDenied());
            }
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    command.Parameters.Add(new NpgsqlParameter("parent_id", parentId));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                    var parameters = this.SetCommandGet(command, parentId, withDeleted, credentials);
                    var ctx = new ReferenceServiceExecutionContext
                    {
                        Action = ReferenceAction.Get,
                        Parameters = parameters,
                    };
                    IList<T> list;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        list = this.ReadList(reader, ctx);
                    }
                    return new ServiceResult<IList<T>>(list);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> EditAsync(
            T newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.Name, newData)
                .AddNotEmptyGuidRule($"{this.Name}.{nameof(newData.Id)}", newData.Id)
                .AddNotEmptyGuidRule(nameof(newData.InstanceId), newData.InstanceId)
                .AddNotEmptyGuidRule(nameof(newData.ParentId), newData.ParentId)
                .AddRule($"{nameof(newData.Action)} != {(int)ReferenceAction.Edit} ({ReferenceAction.Edit.ToString()})", 
                    newData,
                    p => p.Action == ReferenceAction.Edit)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult(validationResult.Error);
            }

            var credentials = this.Accessor.GetUserCredentials();
            if (!this.CanEdit(newData, credentials))
            {
                return new ServiceResult(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var transact = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", newData.Id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                        command.Parameters.Add(new NpgsqlParameter("parent_id", newData.ParentId));
                    
                        this.SetCommandEdit(command, newData, credentials);
                        try
                        {
                            var cnt = await command.ExecuteNonQueryAsync();
                            if (cnt != 1)
                            {
                                transact.Rollback();
                                return new ServiceResult(ErrorFactory.ReferenceEditError(this.Name));
                            }
                        }
                        catch (NpgsqlException)
                        {
                            transact.Rollback();
                            return new ServiceResult(ErrorFactory.ReferenceEditError(this.Name));
                        }
                    }
                    transact.Commit();
                    return new ServiceResult();
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(
            Guid id,
            Guid parentId,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.GetUserCredentials();
            if (!this.CanDelete(id, credentials))
            {
                return new ServiceResult(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var transact = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                        command.Parameters.Add(new NpgsqlParameter("parent_id", parentId));

                        var parameters = this.SetCommandDelete(command, id, credentials);
                        var ctx = new ReferenceServiceExecutionContext
                        {
                            Action = ReferenceAction.Delete,
                            Parameters = parameters,
                        };

                        var success = await this.ExecDeleteAsync(command, ctx);
                        if (success)
                        {
                            transact.Commit();
                            return new ServiceResult();
                        }
                        transact.Rollback();
                        return new ServiceResult(ErrorFactory.ReferenceEntryNotFound(this.Name, id));
                    }
                }
            }
        }


        /// <inheritdoc />
        public async Task<ServiceResult> CreateAsync(
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.CreateAsync(entity, oc);
            return result.Success
                ? new ServiceResult() 
                : new ServiceResult(result.Errors);
        }

        /// <inheritdoc />
        async Task<ServiceResult<IList<ReferenceCollectionBase>>> ICollectionReferenceService.GetAsync(
            Guid parentId,
            bool withDeleted,
            NpgsqlConnectionWrapper oc)
        {
            var result = await this.GetAsync(parentId, withDeleted, oc);
            var list = result.Result?.Cast<ReferenceCollectionBase>().ToList();
            return result.Success
                ? new ServiceResult<IList<ReferenceCollectionBase>>(list) 
                : new ServiceResult<IList<ReferenceCollectionBase>>(result.Errors);
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult> EditAsync(
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.EditAsync(entity, oc);
            return result.Success
                ? new ServiceResult() 
                : new ServiceResult(result.Errors);
        }

        /// <inheritdoc />
        async Task<ServiceResult> ICollectionReferenceService.DeleteAsync(
            Guid id,
            Guid parentId,
            NpgsqlConnectionWrapper oc)
        {
            return await this.DeleteAsync(id, parentId, oc);
        }
        
        #endregion
    }
}