using System;
using DeliveryTracker.Database;
using Npgsql;
using Xunit;

namespace DeliveryTracker.Tests.Database
{
    public class NpgsqlTransactionWrapperTest : DeliveryTrackerTestBase, IDisposable
    {
        private readonly string tableName = $"test_table_{Guid.NewGuid():N}";

        public NpgsqlTransactionWrapperTest() : base()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"drop table if exists {this.tableName};";
                    command.ExecuteNonQuery();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"create table {this.tableName} (id serial primary key, str text);";
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"drop table if exists {this.tableName};";
                    command.ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public void Commit()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                // ReSharper disable once UnusedVariable
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Commit());   
                }
                
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select str from {this.tableName} where str = '{randomString}';";
                    var result = command.ExecuteScalar();
                    Assert.Equal(randomString, result);
                }
            }
        }
        
        [Fact]
        public void DoubleCommit()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Commit());
                    tran.Commit();
                }
                
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select str from {this.tableName} where str = '{randomString}';";
                    var result = command.ExecuteScalar();
                    Assert.Equal(randomString, result);
                }
            }
        }
        
        [Fact]
        public void Rollback()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                // ReSharper disable once UnusedVariable
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Rollback());   
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select str from {this.tableName} where str = '{randomString}';";
                    var result = command.ExecuteScalar();
                    Assert.Null(result);
                }
            }
        }
        
        [Fact]
        public void DoubleRollback()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Rollback());   
                    tran.Rollback();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select str from {this.tableName} where str = '{randomString}';";
                    var result = command.ExecuteScalar();
                    Assert.Null(result);
                }
            }
        }
        
        [Fact]
        public void RollbackAfterCommit()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Commit());   
                    tran.Rollback();
                }
                
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select str from {this.tableName} where str = '{randomString}';";
                    var result = command.ExecuteScalar();
                    Assert.Null(result);
                }
            }
        }

        [Fact]
        public void СommitAfterRollback()
        {
            using (var conn = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                var randomString = Guid.NewGuid().ToString("N");
                conn.Connect();
                using (var tran = conn.BeginTransaction())
                {
                    this.InsertIntoTempTableWithTransaction(conn, randomString, t => t.Rollback());   
                    Assert.Throws<CommitAfterRollbackException>(() => tran.Commit());
                }
            }
        }


        private void InsertIntoTempTableWithTransaction(
            NpgsqlConnectionWrapper oc, 
            string str,
            Action<NpgsqlTransactionWrapper> afterExecuting)
        {
            using (var conn = oc)
            {
                conn.Connect();
                using (var trans = conn.BeginTransaction())
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = $"insert into {this.tableName}(str) values (@str);";
                        command.Parameters.Add(new NpgsqlParameter("str", str));
                        command.ExecuteNonQuery();
                    }

                    afterExecuting(trans);
                }
            }
        }
    }
}