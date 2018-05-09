using DeliveryTracker.Common;

namespace DeliveryTracker.Database
{
    public sealed class DatabaseSettings : ISettings
    {
        public DatabaseSettings(
            string name,
            string defaultConnectionString)
        {
            this.Name = name;
            this.DefaultConnectionString = defaultConnectionString;
        }

        /// <inheritdoc />
        public string Name { get; }
        
        /// <summary>
        /// Стандартная строка подключения.
        /// </summary>
        public string DefaultConnectionString { get; }
    }
}