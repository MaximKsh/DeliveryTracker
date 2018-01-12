using Microsoft.Extensions.Configuration;

namespace DeliveryTracker.Database
{
    public class PostgresConnectionProvider : IPostgresConnectionProvider
    {
        private readonly IConfiguration configuration;

        public PostgresConnectionProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public NpgsqlConnectionWrapper Create()
        {
            return new NpgsqlConnectionWrapper(this.configuration.GetConnectionString("DefaultConnection"));
        }
    }
}