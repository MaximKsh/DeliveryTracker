using DeliveryTracker.Common;

namespace DeliveryTracker.Identification
{
    public sealed class SessionSettings : ISettings
    {
        public SessionSettings(
            string name,
            int userOnlineTimeout,
            int sessionInactiveTimeout)
        {
            this.Name = name;
            this.UserOnlineTimeout = userOnlineTimeout;
            this.SessionInactiveTimeout = sessionInactiveTimeout;
        }

        /// <inheritdoc />
        public string Name { get; }
        
        /// <summary>
        /// Время в минутах от последней активности, пока пользователь еще считается онлайн.
        /// </summary>
        public int UserOnlineTimeout { get; }
        
        /// <summary>
        /// Время в минутах от последней активности, пока сессия считается не устаревшей.
        /// </summary>
        public int SessionInactiveTimeout { get; }
    }
}