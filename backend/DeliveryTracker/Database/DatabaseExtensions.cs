using System;
using System.Data;
using System.Text;
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
        
        public static void AppendIfNotNull(this NpgsqlCommand command, StringBuilder builder, string name, object value)
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
    }
}
