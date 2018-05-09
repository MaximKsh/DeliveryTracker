using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DeliveryTracker.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;

namespace DeliveryTracker.Database
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDeliveryTrackerDatabase(this IServiceCollection services)
        {
            services
                .AddSingleton<IPostgresConnectionProvider, PostgresConnectionProvider>()
                ;

            return services;
        }

        public static ISettingsStorage AddDeliveryTrackerDatabaseSettings(
            this ISettingsStorage storage,
            IConfiguration configuration)
        {
            var dbSettings = DatabaseHelper.ReadDatabaseSettingsFromConf(configuration);
            storage.RegisterSettings(dbSettings);
            return storage;
        }

        public static NpgsqlParameter CanBeNull(this NpgsqlParameter parameter)
        {
            parameter.Value = parameter.Value ?? DBNull.Value;
            parameter.IsNullable = true;
            return parameter;
        }
        
        public static NpgsqlParameter WithType(this NpgsqlParameter parameter, NpgsqlDbType dbType)
        {
            parameter.NpgsqlDbType = dbType;
            return parameter;
        }
        
        public static NpgsqlParameter WithArrayType(this NpgsqlParameter parameter, NpgsqlDbType dbType)
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            parameter.NpgsqlDbType = NpgsqlDbType.Array | dbType;
            return parameter;
        }
        
        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, T defValue = default)
        {
            return !reader.IsDBNull(ordinal) ?
                (T)reader.GetValue(ordinal) : 
                defValue;
        }
        
        public static int AppendIfNotDefault<T>(
            this NpgsqlCommand command, 
            StringBuilder builder, 
            string name, 
            T value,
            string columnName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                return 0;
            }
            if (builder.Length != 0)
            {
                builder.Append(",");
            }
            builder.Append(columnName ?? name);
            builder.Append(" = @");
            builder.Append(name);
            command.Parameters.Add(new NpgsqlParameter(name, value));
            return 1;
        }
    }
}
