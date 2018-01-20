namespace DeliveryTracker.Identification
{
    public interface IUserCredentialsAccessor
    {
        /// <summary>
        /// Получить объект UserCredentials из текущего контекста запроса.
        /// </summary>
        UserCredentials UserCredentials { get; }
    }
}