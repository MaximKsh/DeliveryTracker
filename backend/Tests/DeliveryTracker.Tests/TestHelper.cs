﻿using System;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using Npgsql;

namespace DeliveryTracker.Tests
{
    public static class TestHelper
    {
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

        
        private static readonly string SqlInsertInstance = $@"

insert into instances({InstanceHelper.GetInstanceColumns()})
values ({InstanceHelper.GetInstanceColumns("@")})
returning {InstanceHelper.GetInstanceColumns()}
;
insert into entries_statistics(instance_id)
values (@id)
returning 1
;";

        public const string CorrectPassword = "123Bb!";

        public static Mock<ILogger<T>> CreateLoggerMock<T>()
        {
            var mock = new Mock<ILogger<T>>();
            mock.Setup(p => 
                p.Log(
                    It.IsAny<LogLevel>(), 
                    It.IsAny<EventId>(),
                    It.IsAny<FormattedLogValues>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<FormattedLogValues, Exception, string>>()));
            return mock;
        }
        

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
                    Instance result = null;
                    if (reader.Read())
                    {
                        result = reader.GetInstance();
                    }
                    reader.Read();
                    reader.NextResult();
                    return result;
                }

                return null;
            }
        }

        public static User CreateRandomUser(Guid role, Guid instanceId, NpgsqlConnectionWrapper conn)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SqlCreate;
                command.Parameters.Add(new NpgsqlParameter("id", Guid.NewGuid()));
                command.Parameters.Add(new NpgsqlParameter("code", Guid.NewGuid().ToString().Substring(0, 10)));
                command.Parameters.Add(new NpgsqlParameter("role", role));
                command.Parameters.Add(new NpgsqlParameter("surname", "test_generated"));
                command.Parameters.Add(new NpgsqlParameter("name", "test_generated"));
                command.Parameters.Add(new NpgsqlParameter("patronymic", "test_generated"));
                command.Parameters.Add(new NpgsqlParameter("phone_number", "test_generated"));
                command.Parameters.Add(new NpgsqlParameter("instance_id", instanceId));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetUser();
                    }
                }
            }

            return null;
        }
        
    }
}