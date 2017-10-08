namespace DeliveryTracker.Helpers
{
    public static class CacheHelper
    {
        /// <summary>
        /// Формат для ключа кэша IMemoryCache
        /// 0 - Имя типа, работающего с кэшом
        /// 1 - Имя для объекта кэша
        /// </summary>
        public const string CacheKeyFormat = "CacheKey_{0}_{1}";
    }
}