using System;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Validation;
using Npgsql;

namespace DeliveryTracker.Identification
{
    public sealed class DeviceManager : IDeviceManager
    {
        #region sql

        private const string SqlUpdateDevice = @"
update sessions
set device_type = @device_type,
    device_version = @device_version,
    application_type = @app_type,
    application_version = @app_version,
    language = @lang,
    firebase_id = @firebase_id
where user_id = @user_id
;
";
        
        private static readonly string SqlSelectDevice = $@"
select {IdentificationHelper.GetDeviceColumns()}
from sessions 
where user_id = @user_id
;
";
        
        #endregion
        
        #region fields

        private readonly IPostgresConnectionProvider cp;
        
        #endregion
        
        #region constuctor

        public DeviceManager(
            IPostgresConnectionProvider cp)
        {
            this.cp = cp;
        }
        
        #endregion
        
        #region implementation
        
        public async Task<ServiceResult> UpdateUserDeviceAsync(
            Device device,
            NpgsqlConnectionWrapper oc = null)
        {
            using(var conn = oc?.Connect() ?? this.cp.Create().Connect())
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SqlUpdateDevice;
                command.Parameters.Add(new NpgsqlParameter("user_id", device.UserId));
                command.Parameters.Add(new NpgsqlParameter("device_type", device.Type).CanBeNull());
                command.Parameters.Add(new NpgsqlParameter("device_version", device.Version).CanBeNull());
                command.Parameters.Add(new NpgsqlParameter("app_type", device.ApplicationType).CanBeNull());
                command.Parameters.Add(new NpgsqlParameter("app_version", device.ApplicationVersion).CanBeNull());
                command.Parameters.Add(new NpgsqlParameter("lang", device.Language).CanBeNull());
                command.Parameters.Add(new NpgsqlParameter("firebase_id", device.FirebaseId).CanBeNull());

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0
                    ? new ServiceResult()
                    : new ServiceResult(ErrorFactory.UserNotFound(device.UserId));
            }
        }

        public async Task<ServiceResult<Device>> GetUserDeviceAsync(
            Guid userId,
            NpgsqlConnectionWrapper oc = null)
        {
            using(var conn = oc?.Connect() ?? this.cp.Create().Connect())
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SqlSelectDevice;
                command.Parameters.Add(new NpgsqlParameter("user_id", userId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ServiceResult<Device>(reader.GetDevice());
                    }
                    return new ServiceResult<Device>(ErrorFactory.UserNotFound(userId));
                }
            }
        }
        
        #endregion
    }
} 