using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.References
{
    public abstract class ReferenceServiceBase <T> : IReferenceService<T> 
        where T : ReferenceEntityBase, new()
    {
        #region nested types

        protected enum ReferenceExecutionAction
        {
            Create,
            Edit,
            Get,
            Delete,
        }
        
        protected class ExecutionParameters : Dictionary<string, object>
        {}

        protected class ReferenceServiceExecutionContext
        {
            public ReferenceExecutionAction Action { get; set; }
            public ExecutionParameters Parameters { get; set; }

        }
        
        #endregion
        
        #region fields

        private readonly string referenceName = typeof(T).Name;
        
        protected readonly IPostgresConnectionProvider Cp;

        protected readonly IUserCredentialsAccessor Accessor;

        #endregion

        #region constructor
        
        public ReferenceServiceBase(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor)
        {
            this.Cp = cp;
            this.Accessor = accessor;
        }

        #endregion
        
        #region protected

        protected abstract ExecutionParameters SetCommandCreate(
            NpgsqlCommand command, 
            T newData, 
            Guid id, 
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandEdit(
            NpgsqlCommand command, 
            T newData, 
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandGet(
            NpgsqlCommand command, 
            Guid id, 
            UserCredentials credentials);

        protected abstract ExecutionParameters SetCommandDelete(
            NpgsqlCommand command, 
            Guid id, 
            UserCredentials credentials);

        protected abstract T Read(
            IDataReader reader,  
            ReferenceServiceExecutionContext ctx);

        protected virtual async Task<bool> ExecDeleteAsync(
            NpgsqlCommand command, 
            ReferenceServiceExecutionContext ctx)
        {
            return await command.ExecuteNonQueryAsync() == 1;
        }
        
        protected virtual bool CanCreate(T newData, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
        
        protected virtual bool CanEdit(T newData, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
        
        protected virtual bool CanGet(Guid id, UserCredentials credentials)
        {
            return credentials.Valid;
        }
        
        protected virtual bool CanDelete(Guid id, UserCredentials credentials)
        {
            return credentials.Valid
                   && (credentials.Role == DefaultRoles.CreatorRole
                       || credentials.Role == DefaultRoles.ManagerRole);
        }
        
        #endregion
        
        #region public
        
        /// <inheritdoc />
        public abstract string Name { get; }
        
        /// <inheritdoc />
        public abstract ReferenceDescription ReferenceDescription { get; }
        
        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> CreateAsync(
            T newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.referenceName, newData)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<T>(validationResult.Error);
            }

            var credentials = this.Accessor.UserCredentials;
            if (!this.CanCreate(newData, credentials))
            {
                return new ServiceResult<T>(ErrorFactory.AccessDenied());
            }

            T newEntity = null;
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var transact = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        var id = Guid.NewGuid();
                        command.Parameters.Add(new NpgsqlParameter("id", id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                        var parameters = this.SetCommandCreate(command, newData, id, credentials);
                        var ctx = new ReferenceServiceExecutionContext
                        {
                            Action = ReferenceExecutionAction.Create,
                            Parameters = parameters,
                        };
                        try
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    newEntity = this.Read(reader, ctx);
                                }
                            }
                        }
                        catch (NpgsqlException)
                        {
                            transact.Rollback();
                            return new ServiceResult<T>(ErrorFactory.ReferenceCreationError(this.referenceName));
                        }
                    }

                    if (newEntity != null)
                    {
                        transact.Commit();
                    }
                    else
                    {
                        transact.Rollback();
                    }
                }
            }
            return new ServiceResult<T>(newEntity);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> GetAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.UserCredentials;
            if (!this.CanGet(id, credentials))
            {
                return new ServiceResult<T>(ErrorFactory.AccessDenied());
            }

            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.Parameters.Add(new NpgsqlParameter("id", id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                    var parameters = this.SetCommandGet(command, id, credentials);
                    var ctx = new ReferenceServiceExecutionContext
                    {
                        Action = ReferenceExecutionAction.Get,
                        Parameters = parameters,
                    };
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<T>(this.Read(reader, ctx));
                        }
                        return new ServiceResult<T>(ErrorFactory.ReferenceEntryNotFound(this.referenceName, id));
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> EditAsync(T newData, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.referenceName, newData)
                .AddNotEmptyGuidRule($"{this.referenceName}.Id", newData.Id)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<T>(validationResult.Error);
            }

            var credentials = this.Accessor.UserCredentials;
            if (!this.CanEdit(newData, credentials))
            {
                return new ServiceResult<T>(ErrorFactory.AccessDenied());
            }

            T updated = null;
            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var transact = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", newData.Id));
                        command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));
                    
                        var parameters = this.SetCommandEdit(command, newData, credentials);
                        var ctx = new ReferenceServiceExecutionContext
                        {
                            Action = ReferenceExecutionAction.Edit,
                            Parameters = parameters,
                        };
                        try
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    updated = this.Read(reader, ctx);
                                }
                            }
                        }
                        catch (NpgsqlException)
                        {
                            transact.Rollback();
                            return new ServiceResult<T>(ErrorFactory.ReferenceEditError(this.referenceName));
                        }
                    }
                    if (updated != null)
                    {
                        transact.Commit();
                    }
                    else
                    {
                        transact.Rollback();
                    }
                    return new ServiceResult<T>(updated);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult> DeleteAsync(Guid id, NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.UserCredentials;
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

                        var parameters = this.SetCommandDelete(command, id, credentials);
                        var ctx = new ReferenceServiceExecutionContext
                        {
                            Action = ReferenceExecutionAction.Delete,
                            Parameters = parameters,
                        };

                        var success = await this.ExecDeleteAsync(command, ctx);
                        if (success)
                        {
                            transact.Commit();
                            return new ServiceResult();
                        }
                        transact.Rollback();
                        return new ServiceResult(ErrorFactory.ReferenceEntryNotFound(this.referenceName, id));
                    }
                }
            }
        }
        
        public async Task<ServiceResult<ReferenceEntityBase>> CreateAsync(
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.CreateAsync(entity, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntityBase>(result.Result) 
                : new ServiceResult<ReferenceEntityBase>(result.Errors);
        }

        async Task<ServiceResult<ReferenceEntityBase>> IReferenceService.GetAsync(
            Guid id,
            NpgsqlConnectionWrapper oc)
        {
            var result = await this.GetAsync(id, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntityBase>(result.Result) 
                : new ServiceResult<ReferenceEntityBase>(result.Errors);
        }

        public async Task<ServiceResult<ReferenceEntityBase>> EditAsync(
            IDictionary<string, object> newData,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.EditAsync(entity, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntityBase>(result.Result) 
                : new ServiceResult<ReferenceEntityBase>(result.Errors);
        }

        async Task<ServiceResult> IReferenceService.DeleteAsync(
            Guid id,
            NpgsqlConnectionWrapper oc)
        {
            return await this.DeleteAsync(id, oc);
        }
        
        #endregion

    }
}