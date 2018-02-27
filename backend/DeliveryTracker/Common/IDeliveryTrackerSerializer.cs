namespace DeliveryTracker.Common
{
    public interface IDeliveryTrackerSerializer
    {
        /// <summary>
        /// Сериализовать объект в JSON.
        /// </summary>
        /// <param name="obj">Сериализуемый объект</param>
        /// <returns>JSON в виде строки</returns>
        string SerializeJson(
            object obj);

        /// <summary>
        /// Сериализовать объект на основе словаря в JSON.
        /// </summary>
        /// <param name="obj">Сериализуемый объект на основе словаря.</param>
        /// <returns>JSON в виде строки.</returns>
        string SerializeJson(
            IDictionaryObject obj);

        /// <summary>
        /// Десериализовать объект из JSON.
        /// </summary>
        /// <param name="serialized">Строка с JSON-обхъектом</param>
        /// <typeparam name="T">Тип объекта, сериализованного в JSON</typeparam>
        /// <returns>Результирующий объект</returns>
        T DeserializeJson<T>(
            string serialized);
        
        /// <summary>
        /// Десериализовать объект на основе словаря из JSON.
        /// </summary>
        /// <param name="serialized">Строка с JSON-обхъектом</param>
        /// <typeparam name="T">Тип объекта, сериализованного в JSON</typeparam>
        /// <returns>Результирующий объект</returns>
        T DeserializeJsonDictionaryObject<T>(
            string serialized) where T : IDictionaryObject, new();
    }
}