using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.References
{
    public abstract class EntryReferenceServiceBase <T> : ReferenceServiceBase<T>, IReferenceService<T> 
        where T : ReferenceEntryBase, new()
    {
        #region fields

        protected readonly IPostgresConnectionProvider Cp;

        protected readonly IUserCredentialsAccessor Accessor;

        #endregion

        #region constructor

        protected EntryReferenceServiceBase(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor)
        {
            this.Cp = cp;
            this.Accessor = accessor;
        }

        #endregion
        
        #region public
        
        /// <inheritdoc />
        public string Name { get; } = typeof(T).Name;

        /// <inheritdoc />
        public abstract ReferenceDescription ReferenceDescription { get; }
        
        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> CreateAsync(
            T newData,
            bool check = true,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.Name, newData)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<T>(validationResult.Error);
            }

            var credentials = this.Accessor.GetUserCredentials();
            if (check && !this.CanCreate(newData, credentials))
            {
                return new ServiceResult<T>(ErrorFactory.AccessDenied());
            }

            T newEntity = null;
            using (var conn = oc?.Connect() ?? this.Cp.Create().Connect())
            using (var transact = conn.BeginTransaction())
            using (var command = conn.CreateCommand())
            {
                var id = newData.Id != default
                    ? newData.Id
                    : Guid.NewGuid();
                var instanceId = check
                    ? credentials.InstanceId
                    : newData.InstanceId;
                command.Parameters.Add(new NpgsqlParameter("id", id));
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                command.Parameters.Add(new NpgsqlParameter("deleted", false));

                var parameters = this.SetCommandCreate(command, newData, id, credentials);
                var ctx = new ReferenceServiceExecutionContext
                {
                    Action = ReferenceAction.Create,
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
                    return new ServiceResult<T>(ErrorFactory.ReferenceCreationError(this.Name));
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
            return new ServiceResult<T>(newEntity);
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> GetAsync(
            Guid id, 
            bool withDeleted = false,
            bool check = true,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.GetUserCredentials();
            if (check && !this.CanGet(id, credentials))
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

                    var parameters = this.SetCommandGet(command, id, withDeleted, credentials);
                    var ctx = new ReferenceServiceExecutionContext
                    {
                        Action = ReferenceAction.Get,
                        Parameters = parameters,
                    };
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<T>(this.Read(reader, ctx));
                        }
                        return new ServiceResult<T>(ErrorFactory.ReferenceEntryNotFound(this.Name, id));
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<T>>> GetAsync(
            ICollection<Guid> ids,
            bool withDeleted = false,
            bool check = true,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.GetUserCredentials();
            if (check && !this.CanGetList(ids, credentials))
            {
                return new ServiceResult<IList<T>>(ErrorFactory.AccessDenied());
            }

            var idsParam = ids is IList<Guid>
                ? ids
                : ids.ToArray();

            using (var conn = oc ?? this.Cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    command.Parameters.Add(new NpgsqlParameter("ids", idsParam).WithArrayType(NpgsqlDbType.Uuid));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", credentials.InstanceId));

                    var parameters = this.SetCommandGetList(command, ids, withDeleted, credentials);
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
                    var errors = ids
                        .Except(list.Select(p => p.Id))
                        .Select(p => ErrorFactory.ReferenceEntryNotFound(this.Name, p));
                    return new ServiceResult<IList<T>>(list, errors);

                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<T>> EditAsync(
            T newData,
            bool check = true,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(this.Name, newData)
                .AddNotEmptyGuidRule($"{this.Name}.Id", newData.Id)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<T>(validationResult.Error);
            }

            var credentials = this.Accessor.GetUserCredentials();
            if (check && !this.CanEdit(newData, credentials))
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
                            Action = ReferenceAction.Edit,
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
                            return new ServiceResult<T>(ErrorFactory.ReferenceEditError(this.Name));
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
        public virtual async Task<ServiceResult> DeleteAsync(
            Guid id,
            bool check = true,
            NpgsqlConnectionWrapper oc = null)
        {
            var credentials = this.Accessor.GetUserCredentials();
            if (check && !this.CanDelete(id, credentials))
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
        public virtual async Task<ServiceResult<ReferencePackage>> PackAsync(
            T entry,
            NpgsqlConnectionWrapper oc = null)
        {
            var package = new ReferencePackage
            {
                Entry = entry,
            };

            return await Task.FromResult(new ServiceResult<ReferencePackage>(package));
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<ReferencePackage>>> PackAsync(
            ICollection<T> entries,
            NpgsqlConnectionWrapper oc = null)
        {
            var packages = entries
                .Select(entry => new ReferencePackage {Entry = entry})
                .ToList();

            return await Task.FromResult(new ServiceResult<IList<ReferencePackage>>(packages));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferenceEntryBase>> CreateAsync(
            IDictionary<string, object> newData,
            bool check,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.CreateAsync(entity, check, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntryBase>(result.Result) 
                : new ServiceResult<ReferenceEntryBase>(result.Errors);
        }

        /// <inheritdoc />
        async Task<ServiceResult<ReferenceEntryBase>> IReferenceService.GetAsync(
            Guid id,
            bool withDeleted,
            bool check,
            NpgsqlConnectionWrapper oc)
        {
            var result = await this.GetAsync(id, withDeleted, check, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntryBase>(result.Result) 
                : new ServiceResult<ReferenceEntryBase>(result.Errors);
        }
        
        /// <inheritdoc />  
        async Task<ServiceResult<IList<ReferenceEntryBase>>> IReferenceService.GetAsync(
            ICollection<Guid> ids,
            bool withDeleted,
            bool check,
            NpgsqlConnectionWrapper oc)
        {
            var result = await this.GetAsync(ids, withDeleted, check, oc);
            var list = result.Result?.Cast<ReferenceEntryBase>().ToList();
            return list != null
                ? new ServiceResult<IList<ReferenceEntryBase>>(list, result.Errors) 
                : new ServiceResult<IList<ReferenceEntryBase>>(result.Errors);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ReferenceEntryBase>> EditAsync(
            IDictionary<string, object> newData,
            bool check,
            NpgsqlConnectionWrapper oc = null)
        {
            var entity = new T();
            entity.SetDictionary(newData);
            var result = await this.EditAsync(entity, check, oc);
            return result.Success
                ? new ServiceResult<ReferenceEntryBase>(result.Result) 
                : new ServiceResult<ReferenceEntryBase>(result.Errors);
        }

        /// <inheritdoc />
        async Task<ServiceResult> IReferenceService.DeleteAsync(
            Guid id,
            bool check,
            NpgsqlConnectionWrapper oc)
        {
            return await this.DeleteAsync(id, check, oc);
        }
        
        
        /// <inheritdoc />
        public virtual async Task<ServiceResult<ReferencePackage>> PackAsync(
            ReferenceEntryBase entry,
            NpgsqlConnectionWrapper oc = null)
        {
            var package = new ReferencePackage
            {
                Entry = entry,
            };

            return await Task.FromResult(new ServiceResult<ReferencePackage>(package));
        }

        /// <inheritdoc />
        public virtual async Task<ServiceResult<IList<ReferencePackage>>> PackAsync(
            ICollection<ReferenceEntryBase> entries,
            NpgsqlConnectionWrapper oc = null)
        {
            var packages = entries
                .Select(entry => new ReferencePackage {Entry = entry})
                .ToList();

            return await Task.FromResult(new ServiceResult<IList<ReferencePackage>>(packages));
        }
        
        #endregion

    }
}