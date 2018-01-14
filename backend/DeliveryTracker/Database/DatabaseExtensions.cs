using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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

        public static NpgsqlParameter CanBeNull(this NpgsqlParameter parameter)
        {
            parameter.Value = parameter.Value ?? DBNull.Value;
            parameter.IsNullable = true;
            return parameter;
        }
        
        public static T GetValueOrDefault<T>(this IDataReader reader, int ordinal, T defValue = default)
        {
            return !reader.IsDBNull(ordinal) ?
                (T)reader.GetValue(ordinal) : 
                defValue;
        }
    }
}
