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

namespace DeliveryTracker.Identification
{
    public class UserManager : IUserManager
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into users (
    id, 
    code, 
    role,
    instance_id,
    surname, 
    name, 
    patronymic,
    phone_number)
values (
    @id, 
    @code, 
    @role,
    @instance_id,
    @surname, 
    @name, 
    @patronymic,
    @phone_number)
returning " + IdentificationHelper.GetUserColumns() + ";";

        private static readonly string SqlUpdate = @"
update users
set
{0}
where id = @id 
    and instance_id = @instance_id 
    and deleted = false
returning " + IdentificationHelper.GetUserColumns() + ";";

        
        private static readonly string SqlGetWithDeleted = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where id = @id 
    and instance_id = @instance_id
;";
        
        private static readonly string SqlGetListWithDeleted = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where id = ANY(@ids) 
    and instance_id = @instance_id
;";
        
        private static readonly string SqlGet = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where id = @id 
    and instance_id = @instance_id
    and deleted = false
;";
        
        private static readonly string SqlGetList = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where id = ANY(@ids) 
    and instance_id = @instance_id
    and deleted = false
;";
        
        private static readonly string SqlGetByCode = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where code = @code
    and instance_id = @instance_id 
    and deleted = false
;";
        
        private const string SqlGetId = @"
select id
from users
where code = @code
    and instance_id = @instance_id 
    and deleted = false
;";
        
        private const string SqlDelete = @"
update users
set deleted = true
where id = @id and instance_id = @instance_id
;
";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        #endregion
        
        #region constructor
        
        public UserManager(IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }
        
        #endregion
        
        #region public
        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> CreateAsync(User user, NpgsqlConnectionWrapper oc = null)
        {
            if (!DefaultRoles.AllRoles.Contains(user.Role))
            {
                return new ServiceResult<User>(ErrorFactory.RoleNotFound());
            }

            var validationResult = new ParametersValidator()
                .AddNotNullRule("user", user)
                .AddNotEmptyGuidRule("User.Id", user.Id)
                .AddNotNullOrWhitespaceRule("User.Code", user.Code)
                .AddNotEmptyGuidRule("User.InstanceId", user.InstanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<User>(validationResult.Error);
            }
            
            return await this.CreateAsyncInternal(user, oc);
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> EditAsync(User user, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule("User", user)
                .AddNotEmptyGuidRule("User.Id", user.Id)
                .AddNotEmptyGuidRule("User.InstanceId", user.InstanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<User>(validationResult.Error);
            }
            
            return await this.EditAsyncInternal(user, oc);
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(
            Guid userId, 
            Guid instanceId,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule("userId", userId)
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<User>(validationResult.Error);
            }
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = withDeleted
                        ? SqlGetWithDeleted
                        : SqlGet;
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<User>(reader.GetUser());
                        }
                    }
                }
            }
            return new ServiceResult<User>(ErrorFactory.UserNotFound(userId));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IList<User>>> GetAsync(
            ICollection<Guid> userIds,
            Guid instanceId,
            bool withDeleted = false,
            NpgsqlConnectionWrapper oc = null)
        {
            if (userIds.Count == 0)
            {
                return new ServiceResult<IList<User>>(new List<User>());
            }
            
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<IList<User>>(validationResult.Error);
            }
            var idsParam = userIds is IList<Guid>
                ? userIds
                : userIds.ToArray();


            var list = new List<User>(userIds.Count);
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = withDeleted
                        ? SqlGetListWithDeleted
                        : SqlGetList;
                    command.Parameters.Add(new NpgsqlParameter("ids", idsParam).WithArrayType(NpgsqlDbType.Uuid));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(reader.GetUser());
                        }
                    }
                }
            }
            return new ServiceResult<IList<User>>(list);
        }
        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(string code, Guid instanceId, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule("code", code)
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return new ServiceResult<User>(validationResult.Error);
            }
            
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetByCode;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<User>(reader.GetUser());
                        }
                    }
                }
            }
            return new ServiceResult<User>(ErrorFactory.UserNotFound(code));
        }

        
        /// <inheritdoc />
        public async Task<Guid?> GetIdAsync(string code, Guid instanceId, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule("code", code)
                .AddNotEmptyGuidRule("instanceId", instanceId)
                .Validate();
            if (!validationResult.Success)
            {
                return null;
            }
            
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetId;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    return (Guid?)await command.ExecuteScalarAsync();
                }
            }
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid userId, Guid instanceId, NpgsqlConnectionWrapper oc = null)
        {
            var validationResult = new ParametersValidator()
                .AddNotEmptyGuidRule("userId", userId)
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
                    command.CommandText = SqlDelete;
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected == 1 
                        ? new ServiceResult()
                        : new ServiceResult(ErrorFactory.UserNotFound(userId)); 
                }
            }
        }

        #endregion
        
        #region private
        
        private async Task<ServiceResult<User>> CreateAsyncInternal(User user, NpgsqlConnectionWrapper oc = null)
        {
            User newUser = null;
            using (var connWrapper = oc ?? this.cp.Create())
            {
                connWrapper.Connect();
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", user.Id));
                    command.Parameters.Add(new NpgsqlParameter("code", user.Code));
                    command.Parameters.Add(new NpgsqlParameter("role", user.Role));
                    command.Parameters.Add(new NpgsqlParameter("surname", user.Surname).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("name", user.Name).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("patronymic", user.Patronymic).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("phone_number", user.PhoneNumber).CanBeNull());
                    command.Parameters.Add(new NpgsqlParameter("instance_id", user.InstanceId));

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                newUser = reader.GetUser();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<User>(ErrorFactory.UserCreationError());
                    }
                }
            }
            
            return new ServiceResult<User>(newUser);
        }
        
        private async Task<ServiceResult<User>> EditAsyncInternal(User user, NpgsqlConnectionWrapper oc = null)
        {
            User updatedUser = null;
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    var builder = new StringBuilder();
                    var parametersCounter = 0; 
                    
                    command.Parameters.Add(new NpgsqlParameter("id", user.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", user.InstanceId));
                    parametersCounter += command.AppendIfNotDefault(builder, "surname", user.Surname);
                    parametersCounter += command.AppendIfNotDefault(builder, "name", user.Name);
                    parametersCounter += command.AppendIfNotDefault(builder, "patronymic", user.Patronymic);
                    parametersCounter += command.AppendIfNotDefault(builder, "phone_number", user.PhoneNumber);

                    command.CommandText = parametersCounter != 0
                        ? string.Format(SqlUpdate, builder.ToString())
                        : SqlGet;
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                updatedUser = reader.GetUser();
                            }
                        }
                    }
                    catch (NpgsqlException)
                    {
                        return new ServiceResult<User>(ErrorFactory.UserEditError());
                    }
                }
            }

            return updatedUser != null
                ? new ServiceResult<User>(updatedUser)
                : new ServiceResult<User>(ErrorFactory.UserNotFound(user.Id));
        }

        #endregion
        
    }
}