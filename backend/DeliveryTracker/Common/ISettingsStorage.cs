namespace DeliveryTracker.Common
{
    public interface ISettingsStorage
    {
        /// <summary>
        /// Добавить новые настройки.
        /// </summary>
        /// <param name="settings">Настройки</param>
        ISettingsStorage RegisterSettings(
            ISettings settings);

        /// <summary>
        /// Получить настройки или null, если настройки отсутствуют.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ISettings GetSettings(
            string name);

        /// <summary>
        /// Получить настройки или null, если настройки отсутствуют.
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetSettings<T>(
            string name) where T : class, ISettings;

    }
}