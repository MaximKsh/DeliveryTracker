using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public class SettingsStorage: ISettingsStorage
    {
        private readonly ConcurrentDictionary<string, ISettings> settingsDict =
            new ConcurrentDictionary<string, ISettings>();

        /// <inheritdoc />
        public ISettingsStorage RegisterSettings(
            ISettings settings)
        {
            this.settingsDict.TryAdd(settings.Name, settings);
            return this;
        }

        /// <inheritdoc />
        public ISettings GetSettings(
            string name)
        {
            return this.settingsDict.GetValueOrDefault(name, null);
        }

        /// <inheritdoc />
        public T GetSettings<T>(
            string name) where T : class, ISettings
        {
            return this.settingsDict.GetValueOrDefault(name, null) as T;
        }
    }
}