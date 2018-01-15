using System.Data;
using DeliveryTracker.Database;
using Xunit;

namespace DeliveryTracker.Tests.Database
{
    public class NpgsqlConnectionWrapperTest : DeliveryTrackerTestBase
    {
        /// <summary>
        /// Работает коннект.
        /// </summary>
        [Fact]
        public void Connect()
        {
            NpgsqlConnectionWrapper wrapper;
            using (wrapper = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                wrapper.Connect();
                Assert.Equal(ConnectionState.Open, wrapper.Connection.FullState);
            }
            Assert.Equal(ConnectionState.Closed, wrapper.Connection.FullState);
        }
        
        /// <summary>
        /// Обертка работает во вложенных юзингах
        /// Соединение закрывается только после выхода из верхего блока.
        /// </summary>
        [Fact]
        public void NestedConnect()
        {
            NpgsqlConnectionWrapper wrapper;
            using ( wrapper = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                wrapper.Connect();
                using (var wrapper2 = wrapper)
                {
                    wrapper2.Connect();
                    using (var wrapper3 = wrapper)
                    {
                        wrapper3.Connect();
                        Assert.Equal(wrapper.Connection, wrapper2.Connection);
                        Assert.Equal(wrapper2.Connection, wrapper3.Connection);
                        Assert.Equal(wrapper.Connection, wrapper3.Connection);
                    }
                    Assert.Equal(ConnectionState.Open, wrapper.Connection.FullState);
                }
                Assert.Equal(ConnectionState.Open, wrapper.Connection.FullState);
            }
            Assert.Equal(ConnectionState.Closed, wrapper.Connection.FullState);
        }

        /// <summary>
        /// Проверка выполнения запроса во вложенном using-e
        /// </summary>
        [Fact]
        public void NestedQuery()
        {
            using (var wrapper = new NpgsqlConnectionWrapper(this.DefaultConnectionString))
            {
                wrapper.Connect();
                using (var wrapper2 = wrapper)
                {
                    wrapper2.Connect();
                    using (var command = wrapper2.CreateCommand())
                    {
                        command.CommandText = "select count(1) from migrations";
                        // ReSharper disable once AccessToDisposedClosure
                        var ex = Record.Exception(() => command.ExecuteNonQuery());
                        Assert.Null(ex);
                    }
                }
            }
        }
        
    }
}