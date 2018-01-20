namespace DeliveryTracker.Database
{
    public interface IPostgresConnectionProvider
    {
        /// <summary>
        /// Получить соединение на стандартный ConnectionString
        /// Соединение в обертке, что позволяет заключать одно соединение в несколько
        /// блоков using.
        /// </summary>
        /// <returns></returns>
        NpgsqlConnectionWrapper Create();
    }
}