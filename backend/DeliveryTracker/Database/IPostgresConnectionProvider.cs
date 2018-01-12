using Npgsql;

namespace DeliveryTracker.Database
{
    public interface IPostgresConnectionProvider
    {
        NpgsqlConnectionWrapper Create();
    }
}