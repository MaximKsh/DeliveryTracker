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
            this.UserOnlineTimeoutMs = userOnlineTimeout;
            this.SessionInactiveTimeoutMs = sessionInactiveTimeout;
        }

        /// <inheritdoc />
        public string Name { get; }
        
        /// <summary>
        /// Время в миллисекундах от последней активности, пока пользователь еще считается онлайн.
        /// </summary>
        public int UserOnlineTimeoutMs { get; }
        
        /// <summary>
        /// Время в миллисекундах от последней активности, пока сессия считается не устаревшей.
        /// </summary>
        public int SessionInactiveTimeoutMs { get; }
    }
}