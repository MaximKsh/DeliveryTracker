using DeliveryTracker.Database;
using Xunit;

namespace DeliveryTracker.Tests.Database
{
    public class PostgresConnectionProviderTest :  DeliveryTrackerTestBase
    {
        /// <summary>
        /// Тест проверяет, что провайдер дает соединение.
        /// </summary>
        [Fact]
        public void ProvideConnection()
        {
            var provider = new PostgresConnectionProvider(this.Configuration);
            using (var conn = provider.Create())
            {
                conn.Connect();
                using (var command = conn.CreateCommand())
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