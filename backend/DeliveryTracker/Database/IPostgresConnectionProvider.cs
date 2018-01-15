namespace DeliveryTracker.Database
{
    public interface IPostgresConnectionProvider
    {
        NpgsqlConnectionWrapper Create();
    }
}