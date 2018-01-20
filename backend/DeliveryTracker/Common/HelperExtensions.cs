using Microsoft.Extensions.Logging;

namespace DeliveryTracker.Common
{
    public static class HelperExtensions
    {
        #region Logger extensions

        public static void Trace<T>(this ILogger<T> logger, string username, string message)
        {
            logger.LogTrace($"[{username}]: {message}");
        }
        
        #endregion
        
    }
}