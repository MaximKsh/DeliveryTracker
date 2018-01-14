using System;
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
        
        private const string SqlCreate = @"
insert into users (id, code, surname, name, patronymic, phone_number, instance_id)
values(@id, @code, @surname, @name, @patronymic, @phone_number, @instance_id)
returning " + IdentificationHelper.UserColumnList + ";";

        private const string SqlUpdate = @"
update users
set
{0}
where id = @id
returning " + IdentificationHelper.UserColumnList + ";";

        private const string SqlGet = @"
select " + IdentificationHelper.UserColumnList + @"
from users
where id = @id;";
        
        private const string SqlGetByCode = @"
select " + IdentificationHelper.UserColumnList + @"
from users
where code = @code
;";
        
        private const string SqlGetId = @"
select id
from users
where code = @code
;";
        
        private const string SqlDelete = @"
delete from users
where id = @id
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
            User newUser = null;
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("code", user.Code));
                    command.Parameters.Add(new NpgsqlParameter("surname", user.Surname));
                    command.Parameters.Add(new NpgsqlParameter("name", user.Name));
                    command.Parameters.Add(new NpgsqlParameter("patronymic", user.Patronymic));
                    command.Parameters.Add(new NpgsqlParameter("phone_number", user.Patronymic));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", user.InstanceId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            newUser = reader.GetUser();
                        }
                    }
                }
            }
            
            return new ServiceResult<User>(newUser);
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> UpdateAsync(Guid userId, User user, NpgsqlConnectionWrapper oc = null)
        {
            User updatedUser = null;
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    var builder = new StringBuilder();
                     
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    AppendIfNotNull(command, builder, "surname", user.Surname);
                    AppendIfNotNull(command, builder, "name", user.Name);
                    AppendIfNotNull(command, builder, "patronymic", user.Patronymic);
                    AppendIfNotNull(command, builder, "phone_number", user.PhoneNumber);
                    
                    command.CommandText = string.Format(SqlUpdate, builder.ToString());
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedUser = reader.GetUser();
                        }
                    }
                }
            }
            return new ServiceResult<User>(updatedUser);
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult<User>> GetAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGet;
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
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
        public async Task<ServiceResult<User>> GetAsync(string code, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetByCode;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
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
        public async Task<Guid> GetIdAsync(string code, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlGetId;
                    command.Parameters.Add(new NpgsqlParameter("code", code));
                    return (Guid)await command.ExecuteScalarAsync();
                }
            }
        }

        
        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid userId, NpgsqlConnectionWrapper oc = null)
        {
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlDelete;
                    command.Parameters.Add(new NpgsqlParameter("id", userId));
                    await command.ExecuteNonQueryAsync();
                    return new ServiceResult();
                }
            }
        }

        #endregion
        
        #region private
        
        private static void AppendIfNotNull(NpgsqlCommand command, StringBuilder builder, string name, object value)
        {
            if (value == null)
            {
                return;
            }
            if (builder.Length != 0)
            {
                builder.Append(",");
            }
            builder.Append(name);
            builder.Append(" = @");
            builder.Append(name);
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }
        
        #endregion
        
    }
}