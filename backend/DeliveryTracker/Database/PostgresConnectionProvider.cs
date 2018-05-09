
using DeliveryTracker.Common;

namespace DeliveryTracker.Database
{
    /// <inheritdoc />
    public class PostgresConnectionProvider : IPostgresConnectionProvider
    {
        private readonly string connectionString;

        public PostgresConnectionProvider(
            ISettingsStorage settingsStorage)
        {
            this.connectionString = settingsStorage.GetSettings<DatabaseSettings>(SettingsName.Database).DefaultConnectionString;
        }

        /// <inheritdoc />
        public NpgsqlConnectionWrapper Create()
        {
            return new NpgsqlConnectionWrapper(this.connectionString);
        }
    }
}