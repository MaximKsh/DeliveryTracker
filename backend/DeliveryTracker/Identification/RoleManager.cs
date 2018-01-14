using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public class RoleManager : IRoleManager
    {
        #region sql
        
        private const string SqlCreate = @"
insert into roles (" + IdentificationHelper.RoleColumnList + @")
values(@id, @name)
on conflict do nothing
returning " + IdentificationHelper.RoleColumnList + ";";
        
        private const string SqlChangeName = @"
update roles
set name = @name
where id = @id
on conflict do nothing
returning " + IdentificationHelper.RoleColumnList + ";";
        
        private const string SqlGet = @"
select " + IdentificationHelper.RoleColumnList + @"
from roles
where id = @id
;";
        
        private const string SqlDelete = @"
delete from roles
where id = @id
;";

        private const string SqlAddToRole = @"
select 1 from users where id = @user_id;
select 1 from roles where id = @role_id;

insert into role_users(id, user_id, role_id)
values(@id, @user_id, @role_id)
on conlict do nothing;
";
        private const string SqlSelectRoles = @"
select 1 from users where id = @user_id;

select
" + IdentificationHelper.RoleColumnListWithTableAlias + @"
from roles r
join role_users ru on r.id = ru.role_id
where ru.user_id = @user_id
limit @limit
offset @offset
;";

        private const string SqlSelectUsers = @"
select 1 from roles where id = @role_id;

select
" + IdentificationHelper.UserColumnListWithTableAlias + @"
from users u
join role_users ru on u.id = ru.user_id
where ru.role_id = @role_id and u.instance_id = @instance_id
limit @limit
offset @offset
;";

        private const string SqlDeleteFromRole = @"
select 1 from users where id = @user_id;
select 1 from roles where id = @role_id;

delete from role_users
where user_id = @user_id and role_id = @role_id'
";
        
        #endregion
        
        #region fields
        
        private readonly IPostgresConnectionProvider cp;

        #endregion

        #region constructor

        public RoleManager(IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }

        #endregion

        #region public

        /// <inheritdoc />
        public async Task<ServiceResult<Role>> CreateAsync(
            string name,
            NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlCreate;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("name", name));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                           return new ServiceResult<Role>(reader.GetRole());
                        }
                        return new ServiceResult<Role>(ErrorFactory.RoleNameConflict());
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Role>> ChangeNameAsync(
            Guid roleId, 
            string name,
            NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlChangeName;
                    command.Parameters.Add(new NpgsqlParameter("id", roleId));
                    command.Parameters.Add(new NpgsqlParameter("name", name));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<Role>(reader.GetRole());
                        }
                        return new ServiceResult<Role>(ErrorFactory.RoleNameConflict());
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<Role>> GetAsync(Guid roleId, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlGet;
                    command.Parameters.Add(new NpgsqlParameter("id", roleId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ServiceResult<Role>(reader.GetRole());
                        }
                        return new ServiceResult<Role>(ErrorFactory.RoleNotFound());
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteAsync(Guid roleId, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlDelete;
                    command.Parameters.Add(new NpgsqlParameter("id", roleId));
                    await command.ExecuteNonQueryAsync();
                    return new ServiceResult();
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> AddToRoleAsync(Guid userId, Guid roleId, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlAddToRole;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("role_id", roleId));
                    command.Parameters.Add(new NpgsqlParameter("user_id", userId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.UserNotFound(null));
                        }

                        await reader.NextResultAsync();
                        
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.RoleNotFound());
                        }

                        await reader.NextResultAsync();
                        
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.UserAlreadyInRole());
                        }
                    }
                    return new ServiceResult();
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IReadOnlyList<Role>>> GetUserRolesAsync(
            Guid userId,
            int limit = DatabaseHelper.DefaultLimit, 
            int offset = DatabaseHelper.DefaultOffset,
            NpgsqlConnectionWrapper oc = null)
        {
            var roles = new List<Role>();
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlSelectRoles;
                    command.Parameters.Add(new NpgsqlParameter("user_id", userId));
                    command.Parameters.Add(new NpgsqlParameter("limit", limit));
                    command.Parameters.Add(new NpgsqlParameter("offset", offset));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult<IReadOnlyList<Role>>(ErrorFactory.UserNotFound(null));
                        }

                        await reader.NextResultAsync();
                        
                        while (await reader.ReadAsync())
                        {
                            roles.Add(reader.GetRole());
                        }
                    }
                }
            }
            
            return new ServiceResult<IReadOnlyList<Role>>(roles);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IReadOnlyList<User>>> GetUsersInRoleAsync(
            Guid roleId,
            Guid instanceId,
            int limit = DatabaseHelper.DefaultLimit, 
            int offset = DatabaseHelper.DefaultOffset,
            NpgsqlConnectionWrapper oc = null)
        {
            var users = new List<User>();
            using (var connWrapper = oc?.Connect() ?? this.cp.Create().Connect())
            {
                using (var command = connWrapper.CreateCommand())
                {
                    command.CommandText = SqlSelectUsers;
                    command.Parameters.Add(new NpgsqlParameter("role_id", roleId));
                    command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));
                    command.Parameters.Add(new NpgsqlParameter("limit", limit));
                    command.Parameters.Add(new NpgsqlParameter("offset", offset));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult<IReadOnlyList<User>>(ErrorFactory.UserNotFound(null));
                        }

                        await reader.NextResultAsync();
                        
                        while (await reader.ReadAsync())
                        {
                            users.Add(reader.GetUser());
                        }
                    }
                }
            }
            
            return new ServiceResult<IReadOnlyList<User>>(users);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> RemoveFromRoleAsync(Guid userId, Guid roleId, NpgsqlConnectionWrapper oc = null)
        {
            using (var conn = oc ?? this.cp.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqlDeleteFromRole;
                    command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                    command.Parameters.Add(new NpgsqlParameter("role_id", roleId));
                    command.Parameters.Add(new NpgsqlParameter("user_id", userId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.UserNotFound(null));
                        }

                        await reader.NextResultAsync();
                        
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.RoleNotFound());
                        }

                        await reader.NextResultAsync();
                        
                        if (!await reader.ReadAsync())
                        {
                            return new ServiceResult(ErrorFactory.UserNotInRole(null, null));
                        }
                    }
                    return new ServiceResult();
                }
            }
        }

        #endregion
    }
}