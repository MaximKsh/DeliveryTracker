using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Database
{
    public class PostgresConnectionProvider : IPostgresConnectionProvider
    {
        private readonly string connectionString;

        public PostgresConnectionProvider(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// <inheritdoc />
        public NpgsqlConnectionWrapper Create()
        {
            return new NpgsqlConnectionWrapper(this.connectionString);
        }
    }
}