using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public class UserManager : IUserManager
    {
        #region sql
        
        private static readonly string SqlCreate = @"
insert into users (" + IdentificationHelper.GetUserColumns() + @")
values (" + IdentificationHelper.GetUserColumns("@") + @")
returning " + IdentificationHelper.GetUserColumns() + ";";

        private static readonly string SqlUpdate = @"
update users
set
{0}
where id = @id and instance_id = @instance_id
returning " + IdentificationHelper.GetUserColumns() + ";";

        private static readonly string SqlGet = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where id = @id and instance_id = @instance_id;";
        
        private static readonly string SqlGetByCode = @"
select " + IdentificationHelper.GetUserColumns() + @"
from users
where code = @code and instance_id = @instance_id
;";
        
        private const string SqlGetId = @"
select id
from users
where code = @code and instance_id = @instance_id
;";
        
        private const string SqlDelete = @"
delete from users
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
        public async Task<ServiceResult<User>> GetAsync(Guid userId, Guid instanceId, NpgsqlConnectionWrapper oc = null)
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
                    command.CommandText = SqlGet;
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
                     
                    command.Parameters.Add(new NpgsqlParameter("id", user.Id));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", user.InstanceId));
                    command.AppendIfNotDefault(builder, "surname", user.Surname);
                    command.AppendIfNotDefault(builder, "name", user.Name);
                    command.AppendIfNotDefault(builder, "patronymic", user.Patronymic);
                    command.AppendIfNotDefault(builder, "phone_number", user.PhoneNumber);
                    
                    command.CommandText = string.Format(SqlUpdate, builder.ToString());
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