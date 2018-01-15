using System;
using DeliveryTracker.Database;
using DeliveryTracker.Instances;
using Npgsql;

namespace DeliveryTracker.Tests
{
    public static class TestHelper
    {
        private static readonly string SqlInsertInstance = $@"
insert into instances({InstanceHelper.GetInstanceColumns()})
values ({InstanceHelper.GetInstanceColumns("@")})
returning {InstanceHelper.GetInstanceColumns()}
;";

        public static Instance CreateRandomInstance(NpgsqlConnectionWrapper conn)
        {
            using (var command = conn.CreateCommand())
            {
                var id = Guid.NewGuid();
                command.CommandText = SqlInsertInstance;
                command.Parameters.Add(new NpgsqlParameter("id", id));
                command.Parameters.Add(new NpgsqlParameter("name", id.ToString("N")));
                command.Parameters.Add(new NpgsqlParameter("creator_id", Guid.Empty));
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInstance();
                    }
                }

                return null;
            }
        }
        
    }
}